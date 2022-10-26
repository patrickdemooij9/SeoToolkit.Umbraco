using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Config.Models
{
    public class MetaFieldsAppSettingsModel
    {
        public string[] SupportedMediaTypes { get; set; } = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        public string OpenGraphCropAlias { get; set; } = "openGraphImage";
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}
