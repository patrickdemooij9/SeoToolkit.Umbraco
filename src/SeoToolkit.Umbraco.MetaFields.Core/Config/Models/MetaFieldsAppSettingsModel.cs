using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Config.Models
{
    public class MetaFieldsAppSettingsModel
    {
        public string[] SupportedMediaTypes { get; set; } = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        public string OpenGraphCropAlias { get; set; } = "openGraphImage";
        public string[] DisabledModules { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Determines if the meta field keywords should be shown. Most search engines don't use it anymore.
        /// </summary>
        public bool ShowKeywordsField { get; set; }
    }
}
