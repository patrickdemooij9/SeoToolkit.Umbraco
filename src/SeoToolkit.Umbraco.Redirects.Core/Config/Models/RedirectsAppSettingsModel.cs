using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Config.Models
{
    public class RedirectsAppSettingsModel
    {
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}
