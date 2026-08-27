#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    /// <summary>What a site says about itself before the crawl begins.</summary>
    public sealed class SiteEntryPoints
    {
        public required RobotsTxtMatcher Robots { get; init; }
        public required IReadOnlyList<Uri> SitemapUrls { get; init; }
    }

    public interface ISiteEntryPointProvider
    {
        Task<SiteEntryPoints> DiscoverAsync(Uri startingUrl, CrawlOptions options, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Fetches robots.txt and the sitemaps, over http, from the site actually being crawled.
    /// <para>
    /// Deliberately not wired to the toolkit's own RobotsTxt and Sitemap packages. On a decoupled
    /// setup the files that govern the crawl are the ones the frontend host serves, which may not
    /// be what Umbraco would generate - and reading them over http is the only way to find out
    /// what a search engine would actually see.
    /// </para>
    /// </summary>
    public sealed class HttpSiteEntryPointProvider : ISiteEntryPointProvider
    {
        private const int MaxSitemapIndexDepth = 3;

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public HttpSiteEntryPointProvider(HttpClient httpClient, ILogger? logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task<SiteEntryPoints> DiscoverAsync(Uri startingUrl, CrawlOptions options, CancellationToken cancellationToken)
        {
            if (startingUrl is null) throw new ArgumentNullException(nameof(startingUrl));
            options ??= new CrawlOptions();

            var robots = await LoadRobotsAsync(startingUrl, options, cancellationToken).ConfigureAwait(false);

            var sitemapUrls = new List<Uri>();
            if (options.SeedFromSitemap)
            {
                var candidates = new List<Uri>();

                foreach (var declared in robots.Sitemaps)
                {
                    if (Uri.TryCreate(declared, UriKind.Absolute, out var url)) candidates.Add(url);
                }

                // Fall back to the conventional location when robots.txt does not say.
                if (candidates.Count == 0)
                    candidates.Add(new Uri(new Uri(startingUrl.GetLeftPart(UriPartial.Authority)), "/sitemap.xml"));

                foreach (var candidate in candidates)
                    await CollectSitemapUrlsAsync(candidate, sitemapUrls, 0, cancellationToken).ConfigureAwait(false);
            }

            return new SiteEntryPoints { Robots = robots, SitemapUrls = sitemapUrls };
        }

        private async Task<RobotsTxtMatcher> LoadRobotsAsync(Uri startingUrl, CrawlOptions options, CancellationToken cancellationToken)
        {
            var robotsUrl = new Uri(new Uri(startingUrl.GetLeftPart(UriPartial.Authority)), "/robots.txt");

            try
            {
                using var response = await _httpClient.GetAsync(robotsUrl, cancellationToken).ConfigureAwait(false);

                // No robots.txt means no restrictions, which is not an error worth surfacing.
                if (!response.IsSuccessStatusCode) return RobotsTxtMatcher.AllowAll;

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return RobotsTxtMatcher.Parse(content);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogInformation(ex, "Could not read {RobotsUrl}; continuing without restrictions.", robotsUrl);
                return RobotsTxtMatcher.AllowAll;
            }
        }

        /// <summary>
        /// Reads a sitemap, following sitemap indexes to a bounded depth so a self-referencing
        /// index cannot send this round in circles.
        /// </summary>
        private async Task CollectSitemapUrlsAsync(Uri sitemapUrl, List<Uri> into, int depth, CancellationToken cancellationToken)
        {
            if (depth > MaxSitemapIndexDepth) return;

            try
            {
                using var response = await _httpClient.GetAsync(sitemapUrl, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) return;

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var document = XDocument.Parse(content);

                if (document.Root is null) return;

                XNamespace ns = document.Root.GetDefaultNamespace();
                var isIndex = document.Root.Name.LocalName.Equals("sitemapindex", StringComparison.OrdinalIgnoreCase);

                foreach (var element in document.Root.Elements(ns + (isIndex ? "sitemap" : "url")))
                {
                    var location = element.Element(ns + "loc")?.Value;
                    if (string.IsNullOrWhiteSpace(location)) continue;
                    if (!Uri.TryCreate(location!.Trim(), UriKind.Absolute, out var url)) continue;

                    if (isIndex)
                        await CollectSitemapUrlsAsync(url, into, depth + 1, cancellationToken).ConfigureAwait(false);
                    else
                        into.Add(url);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A missing or malformed sitemap is a finding for a check to report, not a
                // reason to abandon the crawl before it starts.
                _logger.LogInformation(ex, "Could not read the sitemap at {SitemapUrl}.", sitemapUrl);
            }
        }
    }
}
