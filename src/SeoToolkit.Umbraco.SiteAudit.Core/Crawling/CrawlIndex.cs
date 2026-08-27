#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    /// <summary>
    /// The default site-wide index.
    /// <para>
    /// Memory is the whole design constraint here. Nothing page sized is retained: each crawled
    /// resource is reduced to a <see cref="ResourceSummary"/>, and the link graph is stored as
    /// integer adjacency rather than urls. A ten thousand page crawl costs single-digit
    /// megabytes, so the index is never what decides how large a site can be audited.
    /// </para>
    /// <para>
    /// The expensive derived views - the reverse link index and the duplicate groupings - are
    /// built on first use and cached. Most runs never ask for them, and when several duplicate
    /// checks all ask, they share one grouping pass instead of each walking the corpus.
    /// </para>
    /// <para>
    /// Writes are safe from the crawler's worker threads. Derived views are meant to be read
    /// after the crawl finishes; reading one mid-crawl gives a snapshot, not a live view.
    /// </para>
    /// </summary>
    public sealed class CrawlIndex : ICrawlIndex
    {
        private readonly ConcurrentDictionary<string, int> _idsByUrl = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<int, ResourceSummary> _summaries = new();
        private readonly ConcurrentDictionary<int, int[]> _outlinks = new();
        private readonly HashSet<string> _sitemapUrls = new(StringComparer.Ordinal);
        private readonly HashSet<string> _umbracoUrls = new(StringComparer.Ordinal);
        private readonly object _urlSetLock = new();
        private readonly object _derivedLock = new();
        private readonly Func<Uri, bool>? _robotsAllows;

        private int _nextId = -1;

        // Derived views, built once on demand.
        private Dictionary<int, int[]>? _inlinks;
        private readonly Dictionary<DuplicateFacet, List<IReadOnlyList<ResourceSummary>>> _duplicateCache = new();

        public CrawlIndex(Func<Uri, bool>? robotsAllows = null)
        {
            _robotsAllows = robotsAllows;
        }

        public int Count => _summaries.Count;

        public IEnumerable<ResourceSummary> All => _summaries.Values;

        public IReadOnlyCollection<string> SitemapUrls
        {
            get { lock (_urlSetLock) { return _sitemapUrls.ToArray(); } }
        }

        public IReadOnlyCollection<string> UmbracoUrls
        {
            get { lock (_urlSetLock) { return _umbracoUrls.ToArray(); } }
        }

        /// <summary>
        /// Reserves the id for a url, whether or not it has been crawled. Urls that are only
        /// ever linked to still need an identity, otherwise the graph cannot represent a link
        /// to a page that was never reached - which is exactly what broken link and orphan
        /// analysis is about.
        /// </summary>
        public int GetOrCreateId(string normalizedUrl)
        {
            if (normalizedUrl is null) throw new ArgumentNullException(nameof(normalizedUrl));

            return _idsByUrl.GetOrAdd(normalizedUrl, _ => Interlocked.Increment(ref _nextId));
        }

        /// <summary>
        /// Records a crawled resource and the urls it links to.
        /// <para>
        /// <see cref="ResourceSummary.Id"/> must have come from <see cref="GetOrCreateId"/> for
        /// the same url. The index is the only allocator of ids, because a url can be linked to
        /// before it is crawled and both paths have to agree on its identity - if they disagree
        /// the link graph silently points at the wrong nodes, so this fails loudly instead.
        /// </para>
        /// </summary>
        public void Add(ResourceSummary summary, IReadOnlyCollection<string>? outlinkNormalizedUrls = null)
        {
            if (summary is null) throw new ArgumentNullException(nameof(summary));

            var registeredId = GetOrCreateId(summary.NormalizedUrl);
            if (registeredId != summary.Id)
            {
                throw new InvalidOperationException(
                    $"Resource id {summary.Id} does not match the id {registeredId} already assigned to " +
                    $"'{summary.NormalizedUrl}'. Obtain ids from {nameof(GetOrCreateId)}.");
            }

            _summaries[summary.Id] = summary;

            InvalidateDerived();

            if (outlinkNormalizedUrls is null || outlinkNormalizedUrls.Count == 0)
                return;

            // De-duplicate here: a page linking to the same target ten times is one edge, and
            // storing it ten times would inflate the graph for no analytical benefit.
            var targets = new HashSet<int>();
            foreach (var url in outlinkNormalizedUrls)
                targets.Add(GetOrCreateId(url));

            var edges = new int[targets.Count];
            targets.CopyTo(edges);
            _outlinks[summary.Id] = edges;
        }

        public void AddSitemapUrls(IEnumerable<string> normalizedUrls)
        {
            if (normalizedUrls is null) return;
            lock (_urlSetLock)
            {
                foreach (var url in normalizedUrls) _sitemapUrls.Add(url);
            }
        }

        public void AddUmbracoUrls(IEnumerable<string> normalizedUrls)
        {
            if (normalizedUrls is null) return;
            lock (_urlSetLock)
            {
                foreach (var url in normalizedUrls) _umbracoUrls.Add(url);
            }
        }

        public bool TryGet(string normalizedUrl, [MaybeNullWhen(false)] out ResourceSummary summary)
        {
            summary = null;
            return normalizedUrl is not null
                && _idsByUrl.TryGetValue(normalizedUrl, out var id)
                && _summaries.TryGetValue(id, out summary);
        }

        public bool TryGetById(int id, [MaybeNullWhen(false)] out ResourceSummary summary)
            => _summaries.TryGetValue(id, out summary);

        public IReadOnlyList<int> OutlinksFrom(int resourceId)
            => _outlinks.TryGetValue(resourceId, out var links) ? links : Array.Empty<int>();

        public IReadOnlyList<int> InlinksTo(int resourceId)
        {
            var inlinks = EnsureInlinks();
            return inlinks.TryGetValue(resourceId, out var links) ? links : Array.Empty<int>();
        }

        public IEnumerable<IReadOnlyList<ResourceSummary>> DuplicatesBy(DuplicateFacet facet)
        {
            lock (_derivedLock)
            {
                if (_duplicateCache.TryGetValue(facet, out var cached))
                    return cached;

                var groups = BuildDuplicateGroups(facet);
                _duplicateCache[facet] = groups;
                return groups;
            }
        }

        public bool IsAllowedByRobots(Uri url) => _robotsAllows is null || _robotsAllows(url);

        /// <summary>
        /// Built lazily: the reverse graph is the same size as the forward one, and most runs
        /// never ask a "what links here" question.
        /// </summary>
        private Dictionary<int, int[]> EnsureInlinks()
        {
            lock (_derivedLock)
            {
                if (_inlinks is not null) return _inlinks;

                var builder = new Dictionary<int, List<int>>();
                foreach (var pair in _outlinks)
                {
                    foreach (var target in pair.Value)
                    {
                        if (!builder.TryGetValue(target, out var sources))
                        {
                            sources = new List<int>(2);
                            builder[target] = sources;
                        }
                        sources.Add(pair.Key);
                    }
                }

                _inlinks = builder.ToDictionary(it => it.Key, it => it.Value.ToArray());
                return _inlinks;
            }
        }

        /// <summary>
        /// Groups successful internal html pages sharing a facet value. Errors and non-page
        /// resources are excluded: two 404s sharing a title is not a duplicate content problem,
        /// and reporting it as one would bury the real findings.
        /// </summary>
        private List<IReadOnlyList<ResourceSummary>> BuildDuplicateGroups(DuplicateFacet facet)
        {
            var buckets = new Dictionary<string, List<ResourceSummary>>(StringComparer.Ordinal);

            foreach (var summary in _summaries.Values)
            {
                if (!summary.IsSuccess || summary.Kind != ResourceKind.HtmlPage || !summary.IsInternal)
                    continue;

                var key = GetFacetKey(summary, facet);
                if (string.IsNullOrEmpty(key)) continue;

                if (!buckets.TryGetValue(key!, out var bucket))
                {
                    bucket = new List<ResourceSummary>(2);
                    buckets[key!] = bucket;
                }
                bucket.Add(summary);
            }

            var result = new List<IReadOnlyList<ResourceSummary>>();
            foreach (var bucket in buckets.Values)
            {
                if (bucket.Count < 2) continue;
                bucket.Sort(static (a, b) => a.Id.CompareTo(b.Id));
                result.Add(bucket);
            }
            return result;
        }

        private static string? GetFacetKey(ResourceSummary summary, DuplicateFacet facet) => facet switch
        {
            DuplicateFacet.Title => summary.Title,
            DuplicateFacet.MetaDescription => summary.MetaDescription,
            DuplicateFacet.H1 => summary.H1,
            // Body text is never retained, so exact content matching works off the hash instead.
            DuplicateFacet.Content => summary.ContentHash == 0 ? null : summary.ContentHash.ToString(),
            _ => null
        };

        private void InvalidateDerived()
        {
            // Only matters when something read a derived view mid-crawl, which is not the
            // intended usage. Cheap enough to guard unconditionally.
            if (_inlinks is null && _duplicateCache.Count == 0) return;

            lock (_derivedLock)
            {
                _inlinks = null;
                _duplicateCache.Clear();
            }
        }
    }
}
