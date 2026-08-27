#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>An image referenced by a page.</summary>
    public sealed class DiscoveredImage
    {
        public DiscoveredImage(Uri url,
            string? alt = null,
            bool hasAltAttribute = false,
            int? width = null,
            int? height = null,
            string? loading = null,
            string? srcSet = null)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Alt = alt;
            HasAltAttribute = hasAltAttribute;
            Width = width;
            Height = height;
            Loading = loading;
            SrcSet = srcSet;
        }

        public Uri Url { get; }

        /// <summary>Value of the alt attribute. Empty string is meaningful - it marks a decorative image.</summary>
        public string? Alt { get; }

        /// <summary>
        /// Whether an alt attribute was present at all. A missing alt and an intentionally empty
        /// alt are different problems, so the two are tracked separately.
        /// </summary>
        public bool HasAltAttribute { get; }

        public int? Width { get; }
        public int? Height { get; }

        /// <summary>The loading attribute, e.g. "lazy" or "eager".</summary>
        public string? Loading { get; }

        public string? SrcSet { get; }

        public bool HasDimensions => Width.HasValue && Height.HasValue;

        public bool IsLazyLoaded => string.Equals(Loading, "lazy", StringComparison.OrdinalIgnoreCase);
    }
}
