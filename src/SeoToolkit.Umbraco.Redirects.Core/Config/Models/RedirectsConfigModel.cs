using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Config.Models
{
    public class RedirectsConfigModel
    {
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
        public bool EnableBloomFilter { get; set; } = true;
        public int PermanentRedirectMaxAgeInSeconds { get; set; } = 86400;
    }
}
