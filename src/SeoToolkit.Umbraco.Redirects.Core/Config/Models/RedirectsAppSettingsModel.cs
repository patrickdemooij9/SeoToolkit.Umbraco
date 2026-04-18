using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Config.Models
{
    public class RedirectsAppSettingsModel
    {
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
        public bool EnableBloomFilter { get; set; } = true;
        public string RedirectMiddleWarePosition { get; set; }
    }
}