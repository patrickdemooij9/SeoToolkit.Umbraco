namespace SeoToolkit.Umbraco.Sitemap.Core.Models.ViewModels
{
    public class SitemapContentSettingsViewModel
    {
        /// <summary>When true, this content node is always excluded from the sitemap regardless of other settings.</summary>
        public bool ExcludeFromSitemap { get; set; }

        /// <summary>Per-content override – null means not overridden (inheriting from document type).</summary>
        public string ChangeFrequency { get; set; }

        /// <summary>Per-content override – null means not overridden (inheriting from document type).</summary>
        public double? Priority { get; set; }

        /// <summary>Resolved effective value: content override → document-type setting → null.</summary>
        public string EffectiveChangeFrequency { get; set; }

        /// <summary>Resolved effective value: content override → document-type setting → null.</summary>
        public double? EffectivePriority { get; set; }
    }
}
