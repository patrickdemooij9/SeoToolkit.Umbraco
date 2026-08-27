#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo
{
    /// <summary>
    /// Links to pages on this site that turned out to be broken.
    /// <para>
    /// Answered entirely from the crawl index, with no extra requests: every internal url has
    /// already been fetched, so its status is known. The previous implementation issued a HEAD
    /// request per link per page and blocked a thread on each one, which on a site with shared
    /// navigation meant re-checking the same handful of urls thousands of times.
    /// </para>
    /// <para>
    /// It has to run site-wide rather than per page, because a link may point at a page the crawl
    /// only reaches later.
    /// </para>
    /// </summary>
    public sealed class BrokenInternalLinkCheck : SeoSiteCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "BrokenInternalLink",
            Name = "Broken internal link",
            Description = "A link to a page on this site that does not load.",
            Category = SeoCheckCategory.Links,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 9,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "Links to {Target}, which returns {StatusCode}."
            }
        };

        protected override void Evaluate(SeoSiteCheckContext context)
        {
            foreach (var source in context.Index.All)
            {
                if (!source.IsInternal || source.Kind != ResourceKind.HtmlPage) continue;

                foreach (var targetId in context.Index.OutlinksFrom(source.Id))
                {
                    // A target with no summary was never fetched - blocked by robots.txt, past
                    // the page limit, or simply external. Not knowing its status is not the same
                    // as knowing it is broken, so it is left alone.
                    if (!context.Index.TryGetById(targetId, out var target)) continue;

                    if (target.IsSuccess || target.StatusCode == 0) continue;
                    if (target.StatusCode is >= 300 and < 400) continue;

                    context.Report(source.Url)
                        .Data("Target", target.Url.AbsoluteUri)
                        .Data("StatusCode", target.StatusCode)
                        .Key(target.NormalizedUrl)
                        .Related(target.Url);
                }
            }
        }
    }

    /// <summary>
    /// Links to other sites that do not resolve.
    /// <para>
    /// This one genuinely has to make requests, so it declares
    /// <see cref="SeoCheckCapabilities.ExternalRequests"/> and caches per run - a url linked from
    /// every page in the footer is checked once, not once per page.
    /// </para>
    /// </summary>
    public sealed class BrokenExternalLinkCheck : SeoPageCheckBase
    {
        /// <summary>
        /// Results for the current crawl. Keyed on the index, which is the run's identity, so
        /// nothing has to remember to clear this when the crawl ends.
        /// </summary>
        private static readonly ConditionalWeakTable<ICrawlIndex, ConcurrentDictionary<string, int>> Cache = new();

        private readonly IHttpClientFactory _httpClientFactory;

        public BrokenExternalLinkCheck(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "BrokenExternalLink",
            Name = "Broken external link",
            Description = "A link to another site that does not load.",
            Category = SeoCheckCategory.Links,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 5,
            RequiredCapabilities = SeoCheckCapabilities.ExternalRequests | SeoCheckCapabilities.CrawlIndex,
            MessageTemplates = new Dictionary<string, string>
            {
                ["Status"] = "Links to {Target}, which returns {StatusCode}.",
                ["Unreachable"] = "Links to {Target}, which could not be reached."
            }
        };

        public override async ValueTask RunAsync(SeoPageCheckContext context, CancellationToken cancellationToken)
        {
            var facts = context.Resource.Facts;
            if (facts is null || context.Index is null) return;

            var cache = Cache.GetOrCreateValue(context.Index);
            var client = _httpClientFactory.CreateClient(CrawlEngineFactory.HttpClientName);

            // One page can link to the same external url more than once; it only needs checking
            // once, and only needs reporting once.
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var link in facts.Links)
            {
                if (link.IsInternal) continue;
                if (link.Url.Scheme != Uri.UriSchemeHttp && link.Url.Scheme != Uri.UriSchemeHttps) continue;
                if (!seen.Add(link.Url.AbsoluteUri)) continue;

                var status = await GetStatusAsync(client, cache, link.Url, cancellationToken).ConfigureAwait(false);

                if (status == 0)
                {
                    context.Report("Unreachable")
                        .Data("Target", link.Url.AbsoluteUri)
                        .Key(link.Url.AbsoluteUri);
                }
                else if (status >= 400)
                {
                    context.Report("Status")
                        .Data("Target", link.Url.AbsoluteUri)
                        .Data("StatusCode", status)
                        .Key(link.Url.AbsoluteUri);
                }
            }
        }

        private static async Task<int> GetStatusAsync(HttpClient client,
            ConcurrentDictionary<string, int> cache,
            Uri url,
            CancellationToken cancellationToken)
        {
            if (cache.TryGetValue(url.AbsoluteUri, out var cached)) return cached;

            var status = await RequestAsync(client, url, cancellationToken).ConfigureAwait(false);

            cache[url.AbsoluteUri] = status;
            return status;
        }

        private static async Task<int> RequestAsync(HttpClient client, Uri url, CancellationToken cancellationToken)
        {
            try
            {
                using var head = new HttpRequestMessage(HttpMethod.Head, url);
                using var response = await client.SendAsync(head, cancellationToken).ConfigureAwait(false);

                // Plenty of servers refuse HEAD outright while serving GET perfectly well, so a
                // refusal is retried rather than reported as a broken link.
                if (response.StatusCode is System.Net.HttpStatusCode.MethodNotAllowed
                    or System.Net.HttpStatusCode.NotImplemented
                    or System.Net.HttpStatusCode.Forbidden)
                {
                    return await GetAsync(client, url, cancellationToken).ConfigureAwait(false);
                }

                return (int)response.StatusCode;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static async Task<int> GetAsync(HttpClient client, Uri url, CancellationToken cancellationToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                return (int)response.StatusCode;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    /// <summary>Images referenced by a page that do not load.</summary>
    public sealed class BrokenImageCheck : SeoSiteCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "BrokenImage",
            Name = "Broken image",
            Description = "An image the page references that does not load.",
            Category = SeoCheckCategory.Images,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 6,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "References {Target}, which returns {StatusCode}."
            }
        };

        protected override void Evaluate(SeoSiteCheckContext context)
        {
            // Images are only fetched when asset crawling is on, so without it there is nothing
            // to judge and the check simply finds nothing rather than guessing.
            foreach (var summary in context.Index.All)
            {
                if (summary.Kind != ResourceKind.Image) continue;
                if (summary.IsSuccess || summary.StatusCode == 0) continue;

                foreach (var sourceId in context.Index.InlinksTo(summary.Id))
                {
                    if (!context.Index.TryGetById(sourceId, out var source)) continue;

                    context.Report(source.Url)
                        .Data("Target", summary.Url.AbsoluteUri)
                        .Data("StatusCode", summary.StatusCode)
                        .Key(summary.NormalizedUrl)
                        .Related(summary.Url);
                }
            }
        }
    }
}
