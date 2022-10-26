using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Config.Models
{
    public class MetaFieldsConfigModel
    {
        public string[] SupportedMediaTypes { get; set; } = Array.Empty<string>();
        public string OpenGraphCropAlias { get; set; }
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}
