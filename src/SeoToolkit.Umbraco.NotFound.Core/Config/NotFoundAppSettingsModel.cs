using System;

namespace SeoToolkit.Umbraco.NotFound.Core.Config
{
    public class NotFoundAppSettingsModel
    {
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}