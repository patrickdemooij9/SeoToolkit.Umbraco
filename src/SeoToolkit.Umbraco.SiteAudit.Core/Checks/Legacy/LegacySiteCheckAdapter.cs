#nullable enable
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Legacy
{
    /// <summary>
    /// Runs a check written against the old <see cref="ISiteCheck"/> api on the new crawler.
    /// <para>
    /// The old interface is synchronous, sees one page at a time, and formats its own messages,
    /// so it cannot simply be a special case of the new one. Rather than break every existing
    /// check, each is wrapped here: the new <see cref="CrawledResource"/> is projected back down
    /// to the model the old api expects, and its results are lifted into issues.
    /// </para>
    /// <para>
    /// This is why the adapter declares <see cref="SeoCheckCapabilities.RawBody"/>. Old checks
    /// expect an HtmlAgilityPack document, so the markup has to be kept and parsed a second time.
    /// That cost is only paid when a legacy check is actually registered - which is also the
    /// clearest argument for porting them.
    /// </para>
    /// </summary>
    public sealed class LegacySiteCheckAdapter : ISeoPageCheck
    {
        /// <summary>Marks issues that came through the adapter, so the UI can flag them.</summary>
        public const string LegacyProviderName = "Legacy (ISiteCheck)";

        private readonly ISiteCheck _inner;
        private SeoCheckDescriptor? _descriptor;

        /// <summary>
        /// One shared context per crawl. The index is the run's identity, and the weak table
        /// means nothing has to remember to clean this up when the crawl ends.
        /// </summary>
        private static readonly ConditionalWeakTable<ICrawlIndex, SiteAuditContext> Contexts = new();

        private static readonly SiteAuditContext Detached = new();

        public LegacySiteCheckAdapter(ISiteCheck inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public ISiteCheck InnerCheck => _inner;

        public SeoCheckDescriptor Descriptor => _descriptor ??= new SeoCheckDescriptor
        {
            Alias = _inner.Alias,
            Name = _inner.Name,
            Description = _inner.Description,
            Category = SeoCheckCategory.Other,
            DefaultSeverity = SeoSeverity.Warning,
            // Old checks carry no weight information, so give them a middling one rather than
            // let them dominate or vanish from the score.
            Weight = 3,
            RequiredCapabilities = SeoCheckCapabilities.RawBody | SeoCheckCapabilities.CrawlIndex,
            AppliesTo = ResourceKinds.HtmlPage,
            ProviderName = LegacyProviderName,
            MessageTemplates = new ConcurrentDictionary<string, string>
            {
                [string.Empty] = _inner.ErrorMessage
            }
        };

        public ValueTask RunAsync(SeoPageCheckContext context, CancellationToken cancellationToken)
        {
            var page = Project(context.Resource);
            if (page is null) return default;

            var legacyContext = context.Index is null ? Detached : Contexts.GetValue(context.Index, CreateContext);

            foreach (var result in _inner.RunCheck(page, legacyContext))
            {
                if (result is null) continue;

                var issue = context.Report(null, MapSeverity(result.Result));

                // The old api formats its own message. Keep it as evidence so nothing is lost,
                // while still routing the wording through the new formatter.
                string? formatted = null;
                try
                {
                    formatted = _inner.FormatMessage(result);
                }
                catch (Exception)
                {
                    // A check that cannot format its own result should not lose the finding.
                }

                if (formatted is not null) issue.Evidence(formatted).Key(formatted);

                if (result.ExtraValues is null) continue;

                foreach (var pair in result.ExtraValues)
                    issue.Data(pair.Key, pair.Value);
            }

            return default;
        }

        /// <summary>
        /// Rebuilds the old page model. Everything it holds is derivable from the new resource,
        /// except the HtmlAgilityPack document, which has to be re-parsed from the raw markup.
        /// </summary>
        private static CrawledPageModel? Project(CrawledResource resource)
        {
            if (resource.RawBody is null) return null;

            HtmlDocument document;
            try
            {
                document = new HtmlDocument();
                document.LoadHtml(resource.RawBody);
            }
            catch (Exception)
            {
                return null;
            }

            var page = new CrawledPageModel(resource.RequestedUrl)
            {
                StatusCode = resource.StatusCode,
                Content = document,
                RequestStarted = resource.RequestStartedUtc.UtcDateTime,
                RequestCompleted = resource.RequestCompletedUtc.UtcDateTime
            };

            if (resource.Facts is not null)
            {
                var links = new Uri[resource.Facts.Links.Count];
                for (var i = 0; i < links.Length; i++) links[i] = resource.Facts.Links[i].Url;
                page.FoundUrls = links;
            }
            else
            {
                page.FoundUrls = Array.Empty<Uri>();
            }

            return page;
        }

        private static SiteAuditContext CreateContext(ICrawlIndex index) => new CrawlIndexSiteAuditContext(index);

        private static SeoSeverity MapSeverity(SiteCrawlResultType result) => result switch
        {
            SiteCrawlResultType.Error => SeoSeverity.Error,
            SiteCrawlResultType.Warning => SeoSeverity.Warning,
            _ => SeoSeverity.Notice
        };

        /// <summary>
        /// Backs the legacy context with the crawl index, so an old check asking "have we seen
        /// this url" gets the real answer without anything copying the whole crawl into a
        /// dictionary. Statuses the check discovers itself are cached alongside, in a collection
        /// that is safe to write to from several crawl threads - unlike the legacy default.
        /// </summary>
        private sealed class CrawlIndexSiteAuditContext : SiteAuditContext
        {
            private readonly ICrawlIndex _index;
            private readonly ConcurrentDictionary<string, int> _discovered = new(StringComparer.Ordinal);

            public CrawlIndexSiteAuditContext(ICrawlIndex index)
            {
                _index = index;
            }

            public override int? GetStatusCode(Uri url)
            {
                if (url is null) return null;

                if (_discovered.TryGetValue(url.AbsoluteUri, out var cached)) return cached;

                return _index.TryGet(url.AbsoluteUri, out var summary) ? summary.StatusCode : null;
            }

            public override void AddUrlIfNotPresent(Uri url, int statusCode)
            {
                if (url is null) return;
                _discovered.TryAdd(url.AbsoluteUri, statusCode);
            }
        }
    }
}
