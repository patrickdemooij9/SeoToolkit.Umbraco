#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    /// <summary>Raised as each resource finishes, so progress can be reported and results stored.</summary>
    public sealed class ResourceCrawledEventArgs : EventArgs
    {
        public required CrawledResource Resource { get; init; }

        /// <summary>
        /// The compact record that went into the index. Carries the derived indexability, so a
        /// consumer storing the result does not have to work it out a second time.
        /// </summary>
        public required ResourceSummary Summary { get; init; }

        public required IReadOnlyList<SeoCheckIssue> Issues { get; init; }
        public required int Completed { get; init; }
        public required int Discovered { get; init; }
    }

    /// <summary>
    /// What one check did over a whole run.
    /// <para>
    /// ApplicableCount is the important one: it is the denominator the health score divides by.
    /// Counting against every crawled resource instead would penalise a site for pages a check
    /// never even looked at.
    /// </para>
    /// </summary>
    public sealed class CheckRunStats
    {
        public required string Alias { get; init; }
        public required string Name { get; init; }
        public required SeoCheckCategory Category { get; init; }
        public required int Weight { get; init; }
        public required SeoSeverity Severity { get; init; }

        /// <summary>False when the check was skipped for the whole run.</summary>
        public bool DidRun { get; set; }

        /// <summary>Resources this check was applicable to.</summary>
        public int ApplicableCount;

        /// <summary>Resources it found at least one problem with.</summary>
        public int FailedCount;

        /// <summary>Total findings, which can exceed FailedCount.</summary>
        public int IssueCount;

        public long DurationMs;
    }

    public sealed class CrawlResult
    {
        public required ICrawlIndex Index { get; init; }
        public required IReadOnlyList<SeoCheckIssue> Issues { get; init; }
        public required IReadOnlyList<CheckRunStats> CheckStats { get; init; }
        public required int Crawled { get; init; }
        public required int Discovered { get; init; }
        public required bool WasCancelled { get; init; }
    }

    /// <summary>
    /// Runs a crawl: fetches resources in parallel, parses each once, runs the enabled checks
    /// against them, maintains the shared index, and finishes with the site-wide checks.
    /// <para>
    /// Replaces the old single-threaded loop that blocked a thread between requests and could
    /// only ever look at one page at a time.
    /// </para>
    /// </summary>
    public sealed class CrawlEngine
    {
        private sealed class IssueSink : ISeoIssueSink
        {
            private readonly System.Collections.Concurrent.ConcurrentQueue<SeoCheckIssue> _issues = new();

            public int Count => _issues.Count;

            public void Add(SeoCheckIssue issue) => _issues.Enqueue(issue);

            public IReadOnlyList<SeoCheckIssue> Drain()
            {
                var result = new List<SeoCheckIssue>(_issues.Count);
                while (_issues.TryDequeue(out var issue)) result.Add(issue);
                return result;
            }
        }

        private readonly IResourceFetcher _fetcher;
        private readonly IUrlNormalizer _normalizer;
        private readonly IPageFactsParser _parser;
        private readonly IHostPolitenessGate _politeness;
        private readonly ILogger _logger;

        public event EventHandler<ResourceCrawledEventArgs>? ResourceCrawled;

        public CrawlEngine(IResourceFetcher fetcher,
            IUrlNormalizer normalizer,
            IPageFactsParser parser,
            IHostPolitenessGate politeness,
            ILogger? logger = null)
        {
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _politeness = politeness ?? throw new ArgumentNullException(nameof(politeness));
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task<CrawlResult> CrawlAsync(Uri startingUrl,
            CrawlOptions options,
            IReadOnlyList<ISeoCheck> checks,
            ISeoAuditRunContext run,
            RobotsTxtMatcher? robots = null,
            IEnumerable<Uri>? additionalSeeds = null,
            CancellationToken cancellationToken = default)
        {
            if (startingUrl is null) throw new ArgumentNullException(nameof(startingUrl));
            if (options is null) throw new ArgumentNullException(nameof(options));
            checks ??= Array.Empty<ISeoCheck>();

            robots ??= RobotsTxtMatcher.AllowAll;

            var pageChecks = checks.OfType<ISeoPageCheck>().ToArray();
            var siteChecks = checks.OfType<ISeoSiteCheck>().ToArray();

            // Work out once what the crawler actually has to capture. If nothing asked for raw
            // bodies or headers, they are never retained - across a large crawl that is the
            // difference between a flat memory profile and an alarming one.
            var required = SeoCheckCapabilitiesExtensions.Aggregate(checks.Select(it => it.Descriptor));

            var scopeHosts = ResolveScopeHosts(startingUrl, options);
            bool IsInternal(Uri url) => scopeHosts.Contains(url.Host);

            var index = new CrawlIndex(url => !options.RespectRobotsTxt ||
                                              robots.IsAllowed(options.UserAgent, url.AbsolutePath));
            var frontier = new CrawlFrontier(options.MaxPages, options.MaxDepth);
            var sink = new IssueSink();

            // Site checks accumulate into collectors created fresh for this run, which is what
            // lets the checks themselves stay stateless singletons.
            var collectors = siteChecks
                .Select(check => (Check: check, Collector: check.CreateCollector()))
                .ToArray();

            var optionsByAlias = checks.ToDictionary(
                it => it.Descriptor.Alias,
                it => SeoCheckOptionsResolver.Resolve(it.Descriptor),
                StringComparer.OrdinalIgnoreCase);

            // Seeded with every check, so one that never ran is still reported as present but
            // skipped rather than silently vanishing from the summary.
            var stats = checks.ToDictionary(
                it => it.Descriptor.Alias,
                it => new CheckRunStats
                {
                    Alias = it.Descriptor.Alias,
                    Name = it.Descriptor.Name,
                    Category = it.Descriptor.Category,
                    Weight = it.Descriptor.Weight,
                    Severity = it.Descriptor.DefaultSeverity,
                    DidRun = CanRun(it.Descriptor, run)
                },
                StringComparer.OrdinalIgnoreCase);

            Seed(frontier, startingUrl, additionalSeeds, options, robots);

            var workers = Enumerable
                .Range(0, Math.Max(1, options.MaxConcurrency))
                .Select(_ => Task.Run(() => WorkerAsync(frontier, index, sink, pageChecks, collectors,
                    optionsByAlias, stats, options, run, robots, required, IsInternal, cancellationToken), CancellationToken.None))
                .ToArray();

            try
            {
                await Task.WhenAll(workers).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Cancellation is a normal way for a crawl to end; whatever was gathered still counts.
            }

            if (!cancellationToken.IsCancellationRequested)
                await RunSiteChecksAsync(siteChecks, collectors, optionsByAlias, stats, index, run, sink, cancellationToken)
                    .ConfigureAwait(false);

            return new CrawlResult
            {
                Index = index,
                Issues = sink.Drain(),
                CheckStats = stats.Values.ToArray(),
                Crawled = frontier.Completed,
                Discovered = frontier.SeenCount,
                WasCancelled = cancellationToken.IsCancellationRequested
            };
        }

        private void Seed(CrawlFrontier frontier, Uri startingUrl, IEnumerable<Uri>? additionalSeeds,
            CrawlOptions options, RobotsTxtMatcher robots)
        {
            Enqueue(frontier, startingUrl, 0, null, DiscoverySource.Seed, options, robots);

            if (additionalSeeds is null) return;

            foreach (var seed in additionalSeeds)
                Enqueue(frontier, seed, 0, null, DiscoverySource.Sitemap, options, robots);
        }

        private bool Enqueue(CrawlFrontier frontier, Uri url, int depth, Uri? referrer,
            DiscoverySource source, CrawlOptions options, RobotsTxtMatcher robots)
        {
            var normalized = _normalizer.Normalize(url);
            if (normalized is null) return false;

            if (options.RespectRobotsTxt && !robots.IsAllowed(options.UserAgent, url.AbsolutePath))
            {
                // Remember it so the same blocked url is not re-evaluated for every page linking to it.
                frontier.MarkSeen(normalized);
                return false;
            }

            return frontier.TryEnqueue(new CrawlRequest
            {
                Url = url,
                NormalizedUrl = normalized,
                Depth = depth,
                Referrer = referrer,
                DiscoveredVia = source
            });
        }

        private async Task WorkerAsync(CrawlFrontier frontier,
            CrawlIndex index,
            IssueSink sink,
            IReadOnlyList<ISeoPageCheck> pageChecks,
            (ISeoSiteCheck Check, ISeoCrawlCollector? Collector)[] collectors,
            IReadOnlyDictionary<string, SeoCheckOptions> optionsByAlias,
            IReadOnlyDictionary<string, CheckRunStats> stats,
            CrawlOptions options,
            ISeoAuditRunContext run,
            RobotsTxtMatcher robots,
            SeoCheckCapabilities required,
            Func<Uri, bool> isInternal,
            CancellationToken cancellationToken)
        {
            await foreach (var request in frontier.Reader.ReadAllAsync(CancellationToken.None).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    frontier.Abort();
                    return;
                }

                try
                {
                    await ProcessAsync(request, frontier, index, sink, pageChecks, collectors,
                        optionsByAlias, stats, options, run, robots, required, isInternal, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Stopping an audit is a normal outcome, not a failure to report.
                    // The finally below still completes this request before we leave.
                    frontier.Abort();
                    return;
                }
                catch (Exception ex)
                {
                    // One bad page must never take the crawl down.
                    _logger.LogError(ex, "Unhandled error while crawling {Url}.", request.Url);
                }
                finally
                {
                    // Always after any enqueueing this page caused, or the crawl can decide it is
                    // finished while work is still being scheduled.
                    frontier.Complete(request);
                }
            }
        }

        private async Task ProcessAsync(CrawlRequest request,
            CrawlFrontier frontier,
            CrawlIndex index,
            IssueSink sink,
            IReadOnlyList<ISeoPageCheck> pageChecks,
            (ISeoSiteCheck Check, ISeoCrawlCollector? Collector)[] collectors,
            IReadOnlyDictionary<string, SeoCheckOptions> optionsByAlias,
            IReadOnlyDictionary<string, CheckRunStats> stats,
            CrawlOptions options,
            ISeoAuditRunContext run,
            RobotsTxtMatcher robots,
            SeoCheckCapabilities required,
            Func<Uri, bool> isInternal,
            CancellationToken cancellationToken)
        {
            await _politeness.WaitAsync(request.Url, cancellationToken).ConfigureAwait(false);

            var fetched = await _fetcher.FetchAsync(request.Url, required, cancellationToken).ConfigureAwait(false);

            var internalResource = isInternal(request.Url);
            var facts = ParseFacts(fetched, request.Url, isInternal, required);

            var resource = fetched with
            {
                NormalizedUrl = request.NormalizedUrl,
                UrlHash = _normalizer.Hash(request.NormalizedUrl),
                Depth = request.Depth,
                Referrers = request.Referrer is null ? Array.Empty<Uri>() : new[] { request.Referrer },
                DiscoveredVia = request.DiscoveredVia,
                IsInternal = internalResource,
                Facts = facts,
                // Only keep the body if something actually asked for it.
                RawBody = required.Requires(SeoCheckCapabilities.RawBody) ? fetched.RawBody : null
            };

            var outlinks = internalResource
                ? ScheduleLinks(resource, frontier, options, robots, isInternal)
                : Array.Empty<string>();

            var resourceId = index.GetOrCreateId(request.NormalizedUrl);
            var summary = BuildSummary(resource, resourceId, robots, options);
            index.Add(summary, outlinks);

            var issues = await RunPageChecksAsync(resource, pageChecks, collectors, optionsByAlias,
                stats, run, sink, index, cancellationToken).ConfigureAwait(false);

            ResourceCrawled?.Invoke(this, new ResourceCrawledEventArgs
            {
                Resource = resource,
                Summary = summary,
                Issues = issues,
                Completed = frontier.Completed,
                Discovered = frontier.SeenCount
            });
        }

        private PageFacts? ParseFacts(CrawledResource fetched, Uri url, Func<Uri, bool> isInternal, SeoCheckCapabilities required)
        {
            if (fetched.Kind != ResourceKind.HtmlPage || string.IsNullOrEmpty(fetched.RawBody))
                return null;

            try
            {
                var xRobots = required.Requires(SeoCheckCapabilities.ResponseHeaders)
                    ? fetched.GetHeader("x-robots-tag")
                    : null;

                return _parser.Parse(fetched.RawBody!, url, isInternal, xRobots);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not parse the markup of {Url}.", url);
                return null;
            }
        }

        /// <summary>
        /// Queues everything this page links to, and returns the normalised targets for the graph.
        /// External links are recorded as edges but never followed.
        /// </summary>
        private IReadOnlyCollection<string> ScheduleLinks(CrawledResource resource, CrawlFrontier frontier,
            CrawlOptions options, RobotsTxtMatcher robots, Func<Uri, bool> isInternal)
        {
            if (resource.Facts is null) return Array.Empty<string>();

            var targets = new List<string>(resource.Facts.Links.Count);

            foreach (var link in resource.Facts.Links)
            {
                var normalized = _normalizer.Normalize(link.Url);
                if (normalized is null) continue;

                targets.Add(normalized);

                if (!isInternal(link.Url)) continue;

                Enqueue(frontier, link.Url, resource.Depth + 1, resource.RequestedUrl,
                    DiscoverySource.Link, options, robots);
            }

            if (options.CrawlAssets)
            {
                foreach (var image in resource.Facts.Images)
                {
                    var normalized = _normalizer.Normalize(image.Url);
                    if (normalized is null) continue;

                    targets.Add(normalized);
                    if (isInternal(image.Url))
                        Enqueue(frontier, image.Url, resource.Depth + 1, resource.RequestedUrl,
                            DiscoverySource.Link, options, robots);
                }
            }

            return targets;
        }

        private static ResourceSummary BuildSummary(CrawledResource resource, int id,
            RobotsTxtMatcher robots, CrawlOptions options)
            => new()
            {
                Id = id,
                NormalizedUrl = resource.NormalizedUrl,
                Url = resource.RequestedUrl,
                UrlHash = resource.UrlHash,
                StatusCode = resource.StatusCode,
                Kind = resource.Kind,
                Depth = resource.Depth,
                IsInternal = resource.IsInternal,
                DiscoveredVia = resource.DiscoveredVia,
                Indexability = DetermineIndexability(resource, robots, options),
                Title = resource.Facts?.Title,
                MetaDescription = resource.Facts?.MetaDescription,
                H1 = resource.Facts?.PrimaryH1,
                ContentHash = resource.Facts?.ContentHash ?? 0,
                WordCount = resource.Facts?.WordCount ?? 0,
                ResponseTimeMs = resource.TotalMs,
                SizeBytes = resource.TransferredBytes,
                RedirectCount = resource.RedirectChain.Count,
                FinalNormalizedUrl = resource.FinalUrl.AbsoluteUri,
                UmbracoContentKey = resource.UmbracoContentKey,
                Culture = resource.Culture
            };

        /// <summary>
        /// Works out once whether a resource can be indexed, so site-wide checks do not each
        /// re-derive the same answer from raw facts.
        /// </summary>
        private static IndexabilityFlags DetermineIndexability(CrawledResource resource,
            RobotsTxtMatcher robots, CrawlOptions options)
        {
            var flags = IndexabilityFlags.None;

            if (!resource.IsInternal) flags |= IndexabilityFlags.External;
            if (resource.IsFailed) flags |= IndexabilityFlags.Unreachable;
            if (resource.IsClientError || resource.IsServerError) flags |= IndexabilityFlags.ErrorStatus;
            if (resource.WasRedirected) flags |= IndexabilityFlags.Redirected;

            if (resource.Facts?.EffectiveRobots.BlocksIndexing() == true)
                flags |= IndexabilityFlags.NoIndex;

            if (options.RespectRobotsTxt && !robots.IsAllowed(options.UserAgent, resource.RequestedUrl.AbsolutePath))
                flags |= IndexabilityFlags.BlockedByRobotsTxt;

            var canonical = resource.Facts?.PrimaryCanonical;
            if (canonical is not null &&
                !string.Equals(canonical.AbsoluteUri, resource.FinalUrl.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
                flags |= IndexabilityFlags.CanonicalisedAway;

            return flags == IndexabilityFlags.None ? IndexabilityFlags.Indexable : flags;
        }

        private async Task<IReadOnlyList<SeoCheckIssue>> RunPageChecksAsync(CrawledResource resource,
            IReadOnlyList<ISeoPageCheck> pageChecks,
            (ISeoSiteCheck Check, ISeoCrawlCollector? Collector)[] collectors,
            IReadOnlyDictionary<string, SeoCheckOptions> optionsByAlias,
            IReadOnlyDictionary<string, CheckRunStats> stats,
            ISeoAuditRunContext run,
            IssueSink sink,
            ICrawlIndex index,
            CancellationToken cancellationToken)
        {
            var perResource = new IssueSink();
            var fanOut = new ForwardingSink(perResource, sink);

            foreach (var check in pageChecks)
            {
                var descriptor = check.Descriptor;

                if (!CanRun(descriptor, run)) continue;
                if (!descriptor.AppliesTo.Matches(resource.Kind)) continue;

                var context = BuildContext(resource, descriptor, optionsByAlias, run, fanOut, index);

                // Count issues raised by this check alone, so a resource can be attributed to the
                // check that flagged it rather than to whatever ran last.
                var before = perResource.Count;
                var startedTicks = Stopwatch.GetTimestamp();

                try
                {
                    await check.RunAsync(context, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // A misbehaving check - quite possibly from a third party package - must not
                    // be able to abort the crawl.
                    _logger.LogError(ex, "Check {Alias} failed on {Url}.", descriptor.Alias, resource.RequestedUrl);
                }

                Record(stats, descriptor.Alias, perResource.Count - before, startedTicks);
            }

            foreach (var (check, collector) in collectors)
            {
                if (collector is null) continue;
                if (!CanRun(check.Descriptor, run)) continue;
                if (!check.Descriptor.AppliesTo.Matches(resource.Kind)) continue;

                var context = BuildContext(resource, check.Descriptor, optionsByAlias, run, fanOut, index);

                var before = perResource.Count;
                var startedTicks = Stopwatch.GetTimestamp();

                try
                {
                    collector.Observe(in context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Collector for {Alias} failed on {Url}.",
                        check.Descriptor.Alias, resource.RequestedUrl);
                }

                Record(stats, check.Descriptor.Alias, perResource.Count - before, startedTicks);
            }

            return perResource.Drain();
        }

        /// <summary>
        /// Records what one check did for one resource. Called from several crawl threads, so
        /// every counter moves through an interlocked update.
        /// </summary>
        private static void Record(IReadOnlyDictionary<string, CheckRunStats> stats, string alias,
            int issuesRaised, long startedTicks)
        {
            if (!stats.TryGetValue(alias, out var entry)) return;

            Interlocked.Increment(ref entry.ApplicableCount);

            if (issuesRaised > 0)
            {
                Interlocked.Increment(ref entry.FailedCount);
                Interlocked.Add(ref entry.IssueCount, issuesRaised);
            }

            var elapsedMs = (Stopwatch.GetTimestamp() - startedTicks) * 1000 / Stopwatch.Frequency;
            Interlocked.Add(ref entry.DurationMs, elapsedMs);
        }

        private static SeoPageCheckContext BuildContext(CrawledResource resource,
            SeoCheckDescriptor descriptor,
            IReadOnlyDictionary<string, SeoCheckOptions> optionsByAlias,
            ISeoAuditRunContext run,
            ISeoIssueSink sink,
            ICrawlIndex index)
            => new(resource,
                descriptor,
                optionsByAlias.TryGetValue(descriptor.Alias, out var options) ? options : SeoCheckOptions.Empty,
                run,
                sink,
                descriptor.RequiredCapabilities.Requires(SeoCheckCapabilities.CrawlIndex) ? index : null);

        private async Task RunSiteChecksAsync(IReadOnlyList<ISeoSiteCheck> siteChecks,
            (ISeoSiteCheck Check, ISeoCrawlCollector? Collector)[] collectors,
            IReadOnlyDictionary<string, SeoCheckOptions> optionsByAlias,
            IReadOnlyDictionary<string, CheckRunStats> stats,
            ICrawlIndex index,
            ISeoAuditRunContext run,
            ISeoIssueSink sink,
            CancellationToken cancellationToken)
        {
            foreach (var check in siteChecks)
            {
                if (!CanRun(check.Descriptor, run)) continue;

                var collector = collectors.FirstOrDefault(it => ReferenceEquals(it.Check, check)).Collector;

                // Site-wide findings are raised here rather than per resource, so they have to be
                // counted here too or the check would look like it never found anything.
                var counting = new IssueSink();
                var context = new SeoSiteCheckContext(
                    check.Descriptor,
                    optionsByAlias.TryGetValue(check.Descriptor.Alias, out var options) ? options : SeoCheckOptions.Empty,
                    run,
                    new ForwardingSink(counting, sink),
                    index,
                    collector);

                try
                {
                    await check.FinalizeAsync(context, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Site check {Alias} failed.", check.Descriptor.Alias);
                }

                if (!stats.TryGetValue(check.Descriptor.Alias, out var entry)) continue;

                entry.IssueCount += counting.Count;

                // Distinct affected urls, so a check reporting twice about one page counts once.
                entry.FailedCount += counting.Drain()
                    .Select(it => it.Url?.AbsoluteUri ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                // A site-wide check without a collector never saw resources one at a time, so it
                // has no applicable count yet. It applies to the whole crawl.
                if (entry.ApplicableCount == 0) entry.ApplicableCount = index.Count;
            }
        }

        /// <summary>
        /// A check runs only if the run can meet everything it declared, and if the feature it
        /// belongs to is present. Both are decided here rather than in every check.
        /// </summary>
        private static bool CanRun(SeoCheckDescriptor descriptor, ISeoAuditRunContext run)
        {
            if (descriptor.RequiresFeature is not null && !run.IsFeatureEnabled(descriptor.RequiresFeature))
                return false;

            return (descriptor.RequiredCapabilities & ~run.AvailableCapabilities) == SeoCheckCapabilities.None;
        }

        private static IReadOnlyCollection<string> ResolveScopeHosts(Uri startingUrl, CrawlOptions options)
        {
            var hosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { startingUrl.Host };

            foreach (var host in options.ScopeHosts) hosts.Add(host);

            return hosts;
        }

        /// <summary>Writes each issue to both the per-resource view and the run-wide collection.</summary>
        private sealed class ForwardingSink : ISeoIssueSink
        {
            private readonly ISeoIssueSink _first;
            private readonly ISeoIssueSink _second;

            public ForwardingSink(ISeoIssueSink first, ISeoIssueSink second)
            {
                _first = first;
                _second = second;
            }

            public void Add(SeoCheckIssue issue)
            {
                _first.Add(issue);
                _second.Add(issue);
            }
        }
    }
}
