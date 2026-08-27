#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Legacy;
using SeoToolkit.Umbraco.SiteAudit.Core.Collections;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    public interface ICrawlEngineFactory
    {
        /// <summary>Builds an engine configured for one run.</summary>
        CrawlEngine Create(CrawlOptions options, RobotsTxtMatcher? robots = null);

        ISiteEntryPointProvider CreateEntryPointProvider();

        /// <summary>
        /// Every check that should run, with checks written against the old api wrapped so they
        /// keep working.
        /// </summary>
        IReadOnlyList<ISeoCheck> GetChecks();

        /// <summary>What the crawler can supply, given what is installed.</summary>
        SeoCheckCapabilities GetAvailableCapabilities();

        /// <summary>
        /// Builds the crawl options for a run, combining the audit's own settings with the
        /// package configuration.
        /// </summary>
        CrawlOptions CreateOptions(Uri startingUrl, int? maxPages, int delayMs, string? baseUrl = null);
    }

    /// <summary>
    /// Assembles the crawl pipeline. Exists because several of the parts depend on per-run
    /// options - the normaliser on the trailing-slash and query rules, the politeness gate on
    /// the delay - so they cannot simply be registered as singletons.
    /// </summary>
    public sealed class CrawlEngineFactory : ICrawlEngineFactory
    {
        /// <summary>Named client for crawling, kept separate from the clients the old checks use.</summary>
        public const string HttpClientName = "SeoToolkit.SiteAudit.Crawler";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SeoCheckCollection _checks;
        private readonly SiteAuditCheckCollection _legacyChecks;
        private readonly ISettingsService<SiteAuditConfigModel> _settings;
        private readonly ILoggerFactory _loggerFactory;

        public CrawlEngineFactory(IHttpClientFactory httpClientFactory,
            SeoCheckCollection checks,
            SiteAuditCheckCollection legacyChecks,
            ISettingsService<SiteAuditConfigModel> settings,
            ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _checks = checks;
            _legacyChecks = legacyChecks;
            _settings = settings;
            _loggerFactory = loggerFactory;
        }

        public CrawlEngine Create(CrawlOptions options, RobotsTxtMatcher? robots = null)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));

            var politeness = new HostPolitenessGate(
                TimeSpan.FromMilliseconds(options.DelayBetweenRequestsMs),
                host => robots?.GetCrawlDelaySeconds(options.UserAgent) is { } seconds
                    ? TimeSpan.FromSeconds(seconds)
                    : null);

            return new CrawlEngine(
                new HttpResourceFetcher(_httpClientFactory.CreateClient(HttpClientName), options),
                new DefaultUrlNormalizer(options),
                new AngleSharpPageFactsParser(),
                politeness,
                _loggerFactory.CreateLogger<CrawlEngine>());
        }

        public ISiteEntryPointProvider CreateEntryPointProvider()
            => new HttpSiteEntryPointProvider(
                _httpClientFactory.CreateClient(HttpClientName),
                _loggerFactory.CreateLogger<HttpSiteEntryPointProvider>());

        public IReadOnlyList<ISeoCheck> GetChecks()
        {
            var configured = _settings.GetSettings().Checks ?? Array.Empty<SiteAuditCheckConfigModel>();

            bool IsEnabled(string alias)
            {
                var setting = configured.FirstOrDefault(it =>
                    string.Equals(it.Alias, alias, StringComparison.OrdinalIgnoreCase));
                return setting is null || setting.Enabled;
            }

            var result = new List<ISeoCheck>();

            foreach (var check in _checks)
            {
                if (IsEnabled(check.Descriptor.Alias)) result.Add(check);
            }

            // Anything still written against the old interface keeps running, wrapped. An alias
            // already claimed by a new check wins, so porting a check simply replaces it.
            var claimed = new HashSet<string>(result.Select(it => it.Descriptor.Alias), StringComparer.OrdinalIgnoreCase);

            foreach (var legacy in _legacyChecks)
            {
                if (claimed.Contains(legacy.Alias)) continue;
                if (!IsEnabled(legacy.Alias)) continue;

                result.Add(new LegacySiteCheckAdapter(legacy));
            }

            return result;
        }

        public SeoCheckCapabilities GetAvailableCapabilities()
        {
            // Everything the core crawler can do by itself. Rendering and screenshots need an
            // add-on to register a provider, so they are deliberately absent - a check that needs
            // them is then reported as unavailable rather than silently passing.
            //
            // UmbracoContent is absent for the same reason. Nothing maps a crawled url back to a
            // node: a crawl can start from a url with no node behind it at all, and a decoupled
            // frontend may route in a way no node url predicts, so any mapping would be a guess.
            // A check declaring it is reported as unavailable, which is the honest answer.
            var capabilities = SeoCheckCapabilities.ResponseHeaders
                               | SeoCheckCapabilities.RawBody
                               | SeoCheckCapabilities.ExternalRequests
                               | SeoCheckCapabilities.CrawlIndex;

            return capabilities;
        }

        /// <summary>
        /// Builds the options for a run from the global configuration plus the audit's own settings.
        /// </summary>
        public CrawlOptions CreateOptions(Uri startingUrl, int? maxPages, int delayMs, string? baseUrl = null)
        {
            var settings = _settings.GetSettings();

            var hosts = new List<string> { startingUrl.Host };
            if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                hosts.Add(baseUri.Host);

            return new CrawlOptions
            {
                MaxPages = maxPages,
                DelayBetweenRequestsMs = Math.Max(delayMs, settings.MinimumDelayBetweenRequest * 1000),
                AllowInvalidCertificates = settings.AllowInvalidCerts,
                ScopeHosts = hosts,
                UserAgent = $"SeoToolkit-SiteAudit/{AssemblyVersionHelper.GetInformationalVersion(typeof(CrawlEngineFactory).Assembly)} " +
                            "(+https://github.com/patrickdemooij9/SeoToolkit.Umbraco)"
            };
        }
    }
}
