#nullable enable
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// What a check needs the crawler to provide. Declaring this earns its keep three ways:
    /// the crawler only does expensive work that something actually asked for, a check whose
    /// needs cannot be met is reported as unavailable rather than silently passing, and an
    /// add-on package can light up capability-dependent checks just by registering a provider.
    /// </summary>
    [Flags]
    public enum SeoCheckCapabilities
    {
        None = 0,

        /// <summary>Needs response headers. Cheap, but not captured unless asked for.</summary>
        ResponseHeaders = 1 << 0,

        /// <summary>
        /// Needs the raw response body. Expensive in memory across a large crawl, so the
        /// crawler discards bodies unless at least one enabled check requests this.
        /// </summary>
        RawBody = 1 << 1,

        /// <summary>Needs markup as rendered by a browser. Requires a rendering add-on.</summary>
        RenderedDom = 1 << 2,

        /// <summary>Will make its own http requests, for example to verify external links.</summary>
        ExternalRequests = 1 << 3,

        /// <summary>Needs the site-wide crawl index.</summary>
        CrawlIndex = 1 << 4,

        /// <summary>Needs crawled urls mapped back to Umbraco content.</summary>
        UmbracoContent = 1 << 5,

        /// <summary>Needs a screenshot of the page. Requires a rendering add-on.</summary>
        Screenshot = 1 << 6
    }

    public static class SeoCheckCapabilitiesExtensions
    {
        public static bool Requires(this SeoCheckCapabilities capabilities, SeoCheckCapabilities capability)
            => (capabilities & capability) == capability;

        /// <summary>
        /// Combines what every enabled check needs into a single mask. The crawler resolves this
        /// once per run and uses it to decide how much work to do per resource.
        /// </summary>
        public static SeoCheckCapabilities Aggregate(IEnumerable<SeoCheckDescriptor> descriptors)
        {
            if (descriptors is null) throw new ArgumentNullException(nameof(descriptors));

            var result = SeoCheckCapabilities.None;
            foreach (var descriptor in descriptors)
                result |= descriptor.RequiredCapabilities;
            return result;
        }
    }
}
