namespace SeoToolkit.Umbraco.Deploy
{
    public static class SeoToolkitDeployConstants
    {
        public static class UdiEntityType
        {
            public const string SeoSetting = "seotoolkit-seo-setting";
            public const string MetaFieldsSetting = "seotoolkit-metafields-setting";
            public const string SitemapPageType = "seotoolkit-sitemap-page-type";
            public const string Script = "seotoolkit-script";
            public const string DomainCollection = "seotoolkit-domain-collection";
            public const string KeyValues = "seotoolkit-key-values";
            public const string MetaFieldsValue = "seotoolkit-metafields-value";
            public const string SitemapContent = "seotoolkit-sitemap-content";
        }

        /// <summary>
        /// Well-known GUID used as the UDI id for the single root (no-domain) key/values artifact.
        /// </summary>
        public static readonly Guid RootKeyValuesGuid = new("5e0a7f2c-9d4b-4c6a-8e1f-3b2a6c9d0e51");
    }
}
