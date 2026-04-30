using System;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    public class SitemapContentSettings
    {
        public Guid NodeKey { get; set; }

        /// <summary>
        /// When true, this content node is always excluded from the sitemap regardless of other settings.
        /// </summary>
        public bool ExcludeFromSitemap { get; set; }

        /// <summary>
        /// Per-content override for ChangeFrequency. Null means no override (inherit from document type).
        /// </summary>
        public string ChangeFrequency { get; set; }

        /// <summary>
        /// Per-content override for Priority. Null means no override (inherit from document type).
        /// </summary>
        public double? Priority { get; set; }
    }
}
