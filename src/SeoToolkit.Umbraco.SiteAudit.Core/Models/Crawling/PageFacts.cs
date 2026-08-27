#nullable enable
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// Everything worth knowing about a page's markup, extracted in a single parse.
    /// <para>
    /// This exists so that a catalogue of dozens of checks costs one parse per page rather
    /// than one query per check per page. Checks must read from here instead of reaching for
    /// the document themselves.
    /// </para>
    /// <para>
    /// A <see cref="PageFacts"/> instance lives only as long as its page is being checked.
    /// Nothing that needs to survive the crawl should hold a reference to it - the memory
    /// footprint of a whole site's worth of page text is not something the crawler can carry.
    /// Site-wide checks accumulate what they need through a collector instead.
    /// </para>
    /// </summary>
    public sealed class PageFacts
    {
        public string? Title { get; init; }

        /// <summary>How many title elements the head contained. More than one is a defect.</summary>
        public int TitleCount { get; init; }

        public string? MetaDescription { get; init; }

        /// <summary>How many meta description tags were present.</summary>
        public int MetaDescriptionCount { get; init; }

        /// <summary>Directives from the robots meta tag.</summary>
        public RobotsDirectives MetaRobots { get; init; }

        /// <summary>Directives from the X-Robots-Tag response header.</summary>
        public RobotsDirectives XRobotsTag { get; init; }

        /// <summary>Combined view of both robots sources, which is what indexability checks want.</summary>
        public RobotsDirectives EffectiveRobots => MetaRobots | XRobotsTag;

        public IReadOnlyList<Uri> Canonicals { get; init; } = Array.Empty<Uri>();

        public IReadOnlyList<string> H1s { get; init; } = Array.Empty<string>();

        /// <summary>All headings in document order, so level skips are detectable.</summary>
        public IReadOnlyList<DiscoveredHeading> Headings { get; init; } = Array.Empty<DiscoveredHeading>();

        public IReadOnlyList<HreflangEntry> HreflangEntries { get; init; } = Array.Empty<HreflangEntry>();

        public SocialCardFacts OpenGraph { get; init; } = SocialCardFacts.Empty;

        public SocialCardFacts TwitterCard { get; init; } = SocialCardFacts.Empty;

        public IReadOnlyList<JsonLdBlock> JsonLdBlocks { get; init; } = Array.Empty<JsonLdBlock>();

        public IReadOnlyList<DiscoveredImage> Images { get; init; } = Array.Empty<DiscoveredImage>();

        public IReadOnlyList<DiscoveredLink> Links { get; init; } = Array.Empty<DiscoveredLink>();

        public IReadOnlyList<Uri> Scripts { get; init; } = Array.Empty<Uri>();

        public IReadOnlyList<Uri> Stylesheets { get; init; } = Array.Empty<Uri>();

        /// <summary>Visible text content, whitespace collapsed. Only valid for the lifetime of this instance.</summary>
        public string? TextContent { get; init; }

        public int WordCount { get; init; }

        /// <summary>Size of the markup in bytes, used by page weight checks.</summary>
        public int HtmlSizeBytes { get; init; }

        /// <summary>Visible text as a fraction of total markup. Low values suggest a template-heavy page.</summary>
        public double TextToHtmlRatio { get; init; }

        /// <summary>
        /// Stable 64 bit hash of the normalised text content. Kept so exact-duplicate detection
        /// can work off the index long after <see cref="TextContent"/> has been released.
        /// </summary>
        public long ContentHash { get; init; }

        public string? Lang { get; init; }

        public string? Viewport { get; init; }

        public Uri? FaviconHref { get; init; }

        public int FormCount { get; init; }

        /// <summary>Set when the page uses a meta refresh redirect.</summary>
        public string? MetaRefresh { get; init; }

        public Uri? PrimaryCanonical => Canonicals.Count > 0 ? Canonicals[0] : null;

        public string? PrimaryH1 => H1s.Count > 0 ? H1s[0] : null;
    }
}
