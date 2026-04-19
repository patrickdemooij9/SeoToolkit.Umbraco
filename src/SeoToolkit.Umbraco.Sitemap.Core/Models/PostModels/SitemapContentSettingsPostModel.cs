using System;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.PostModels
{
    public class SitemapContentSettingsPostModel
    {
        public Guid NodeKey { get; set; }

        /// <summary>When true, this content node is always excluded from the sitemap regardless of other settings.</summary>
        public bool ExcludeFromSitemap { get; set; }

        /// <summary>Null clears a previously set override.</summary>
        public bool? HideFromSitemap { get; set; }

        /// <summary>Null clears a previously set override.</summary>
        public string ChangeFrequency { get; set; }

        /// <summary>Null clears a previously set override.</summary>
        public double? Priority { get; set; }
    }
}
