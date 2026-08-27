#nullable enable
using System;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// The compact record of a crawled resource that survives for the whole run.
    /// <para>
    /// This is deliberately small. A <see cref="Models.Crawling.CrawledResource"/> holds parsed
    /// facts, page text and possibly the raw body; keeping one per page would put a whole
    /// site's markup in memory at once. Instead each resource is reduced to this on the way
    /// out, so the index costs roughly a hundred bytes per page and a large crawl stays flat.
    /// </para>
    /// <para>
    /// Title, description and h1 are kept as text rather than hashes because a duplicate-content
    /// report has to show the offending value, and because these are bounded in size and are
    /// needed for the results grid anyway. Body text is reduced to
    /// <see cref="ContentHash"/> - that one is not bounded.
    /// </para>
    /// </summary>
    public sealed class ResourceSummary
    {
        /// <summary>Dense id assigned in crawl order, used as the node id in the link graph.</summary>
        public required int Id { get; init; }

        public required string NormalizedUrl { get; init; }

        public required Uri Url { get; init; }

        public string? UrlHash { get; init; }

        public int StatusCode { get; init; }

        public ResourceKind Kind { get; init; }

        public int Depth { get; init; }

        public bool IsInternal { get; init; }

        public DiscoverySource DiscoveredVia { get; init; }

        public IndexabilityFlags Indexability { get; init; }

        public string? Title { get; init; }

        public string? MetaDescription { get; init; }

        public string? H1 { get; init; }

        /// <summary>Hash of the normalised body text, for exact duplicate detection.</summary>
        public long ContentHash { get; init; }

        public int WordCount { get; init; }

        public int ResponseTimeMs { get; init; }

        public long SizeBytes { get; init; }

        public int RedirectCount { get; init; }

        /// <summary>Where a redirect ultimately landed, when this resource redirected.</summary>
        public string? FinalNormalizedUrl { get; init; }

        /// <summary>
        /// The node behind this url, if anything ever works one out.
        /// <para>
        /// Always null today, and a check must not depend on it: a crawl can be aimed straight
        /// at a url with no node behind it, and a decoupled frontend may route in a way that no
        /// node url predicts, so any mapping would be a guess. A check that genuinely needs one
        /// should declare <see cref="SeoCheckCapabilities.UmbracoContent"/>, which the crawler
        /// does not offer - so it is reported as unavailable rather than finding nothing.
        /// </para>
        /// </summary>
        public Guid? UmbracoContentKey { get; init; }

        /// <summary>The language of this resource, on the same terms as <see cref="UmbracoContentKey"/>.</summary>
        public string? Culture { get; init; }

        public bool IsIndexable => (Indexability & IndexabilityFlags.Indexable) != 0;

        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    }
}
