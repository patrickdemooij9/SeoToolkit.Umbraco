#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo
{
    /// <summary>
    /// A page that says noindex will not appear in search at all. Usually deliberate, but a
    /// noindex left behind after a staging deployment is one of the most damaging mistakes there
    /// is - which is why this is critical rather than a warning.
    /// </summary>
    public sealed class NoindexCheck : SeoPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "NoindexPage",
            Name = "Page is set to noindex",
            Description = "The page asks search engines not to index it.",
            Category = SeoCheckCategory.Indexability,
            DefaultSeverity = SeoSeverity.Critical,
            Weight = 10,
            RequiredCapabilities = SeoCheckCapabilities.ResponseHeaders,
            MessageTemplates = new Dictionary<string, string>
            {
                ["Meta"] = "This page is excluded from search by a robots meta tag.",
                ["Header"] = "This page is excluded from search by an X-Robots-Tag header."
            }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            var facts = context.Resource.Facts;
            if (facts is null) return;

            // Reported separately because the fix is in a different place: one is in the markup,
            // the other in server or CDN configuration.
            if (facts.MetaRobots.BlocksIndexing()) context.Report("Meta");
            else if (facts.XRobotsTag.BlocksIndexing()) context.Report("Header");
        }
    }

    public sealed class CanonicalMissingCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "CanonicalMissing",
            Name = "Missing canonical",
            Description = "A canonical tag tells search engines which url is the real one.",
            Category = SeoCheckCategory.Indexability,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 6,
            MessageTemplates = new Dictionary<string, string> { [""] = "This page has no canonical tag." }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.Canonicals.Count == 0) context.Report();
        }
    }

    /// <summary>
    /// A canonical pointing elsewhere hands this page's ranking to another url. Legitimate for a
    /// duplicate, and a serious mistake anywhere else - so it is surfaced rather than assumed.
    /// </summary>
    public sealed class CanonicalNonSelfReferencingCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "CanonicalNonSelfReferencing",
            Name = "Canonical points elsewhere",
            Category = SeoCheckCategory.Indexability,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 5,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The canonical points at {Canonical} rather than this page."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            var canonical = facts.PrimaryCanonical;
            if (canonical is null) return;

            var self = context.Resource.FinalUrl;

            // Compared without the fragment, which is never sent to the server anyway.
            var differs = Uri.Compare(canonical, self, UriComponents.HttpRequestUrl,
                UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) != 0;

            if (differs)
            {
                context.Report().Data("Canonical", canonical.AbsoluteUri).Evidence(canonical.AbsoluteUri);
            }
        }
    }

    public sealed class MultipleCanonicalsCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MultipleCanonicals",
            Name = "Multiple canonicals",
            Description = "Conflicting canonical tags are usually ignored altogether.",
            Category = SeoCheckCategory.Indexability,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 7,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page declares {Count} canonical tags."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.Canonicals.Count > 1)
                context.Report().Data("Count", facts.Canonicals.Count);
        }
    }

    /// <summary>A meta refresh is a redirect search engines handle poorly. Use a real one.</summary>
    public sealed class MetaRefreshCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MetaRefreshRedirect",
            Name = "Meta refresh redirect",
            Category = SeoCheckCategory.Indexability,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 5,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page redirects with a meta refresh; a 301 is understood far better."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (!string.IsNullOrWhiteSpace(facts.MetaRefresh))
                context.Report().Evidence(facts.MetaRefresh);
        }
    }

    /// <summary>
    /// A page nothing links to is one visitors cannot reach by browsing, and one search engines
    /// will struggle to find. Needs the whole crawl, so it is a site-wide check.
    /// </summary>
    public sealed class OrphanPageCheck : SeoSiteCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "OrphanPage",
            Name = "Orphan page",
            Description = "Nothing on the site links to this page.",
            Category = SeoCheckCategory.Indexability,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 6,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "No other page on the site links to this one."
            }
        };

        protected override void Evaluate(SeoSiteCheckContext context)
        {
            foreach (var summary in context.Index.All)
            {
                if (!summary.IsInternal || !summary.IsSuccess) continue;
                if (summary.Kind != ResourceKind.HtmlPage) continue;

                // The page the crawl started from is not an orphan; nothing links to it because
                // it is the way in.
                if (summary.DiscoveredVia == DiscoverySource.Seed) continue;

                if (context.Index.InlinksTo(summary.Id).Count == 0)
                    context.Report(summary.Url);
            }
        }
    }
}
