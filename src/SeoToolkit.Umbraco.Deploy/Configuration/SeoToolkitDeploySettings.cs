namespace SeoToolkit.Umbraco.Deploy.Configuration
{
    /// <summary>
    /// Bound from the "SeoToolkit:Deploy" configuration section. Each SeoToolkit entity type has
    /// its own section; a disabled entity type returns no artifacts and skips processing.
    /// </summary>
    public class SeoToolkitDeploySettings
    {
        /// <summary>Per-document-type SEO enable toggle ("seotoolkit-seo-setting").</summary>
        public SeoToolkitDeployEntitySettings SeoSettings { get; set; } = new();

        /// <summary>MetaFields document-type field settings ("seotoolkit-metafields-setting").</summary>
        public SeoToolkitDeployEntitySettings MetaFieldsSettings { get; set; } = new();

        /// <summary>Per-node MetaFields values ("seotoolkit-metafields-value").</summary>
        public SeoToolkitDeployEntitySettings MetaFieldsValues { get; set; } = new();

        /// <summary>Sitemap per-document-type settings ("seotoolkit-sitemap-page-type").</summary>
        public SeoToolkitDeployEntitySettings SitemapPageTypes { get; set; } = new();

        /// <summary>Per-node sitemap overrides ("seotoolkit-sitemap-content").</summary>
        public SeoToolkitDeployEntitySettings SitemapContent { get; set; } = new();

        /// <summary>
        /// Script Manager scripts ("seotoolkit-script"). Disabled by default, as scripts (e.g.
        /// analytics or tag manager ids) usually differ per environment.
        /// </summary>
        public SeoToolkitDeployEntitySettings Scripts { get; set; } = new() { Enabled = false };

        /// <summary>SeoToolkit domain collections ("seotoolkit-domain-collection").</summary>
        public SeoToolkitDeployEntitySettings DomainCollections { get; set; } = new();

        /// <summary>Global / per-domain key-value settings ("seotoolkit-key-values").</summary>
        public SeoToolkitDeployEntitySettings KeyValues { get; set; } = new();

        public bool IsEnabled(string udiEntityType) => udiEntityType switch
        {
            SeoToolkitDeployConstants.UdiEntityType.SeoSetting => SeoSettings.Enabled,
            SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting => MetaFieldsSettings.Enabled,
            SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue => MetaFieldsValues.Enabled,
            SeoToolkitDeployConstants.UdiEntityType.SitemapPageType => SitemapPageTypes.Enabled,
            SeoToolkitDeployConstants.UdiEntityType.SitemapContent => SitemapContent.Enabled,
            SeoToolkitDeployConstants.UdiEntityType.Script => Scripts.Enabled,
            SeoToolkitDeployConstants.UdiEntityType.DomainCollection => DomainCollections.Enabled,
            SeoToolkitDeployConstants.UdiEntityType.KeyValues => KeyValues.Enabled,
            _ => true,
        };
    }

    public class SeoToolkitDeployEntitySettings
    {
        public bool Enabled { get; set; } = true;
    }
}
