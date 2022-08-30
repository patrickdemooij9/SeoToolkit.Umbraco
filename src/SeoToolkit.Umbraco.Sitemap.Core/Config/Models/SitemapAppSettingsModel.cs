using System;

namespace SeoToolkit.Umbraco.Sitemap.Core.Config.Models
{
    public class SitemapAppSettingsModel
    {
        //If alternate pages is turned on, it'll show the alternate pages for the different languages.
        public bool ShowAlternatePages { get; set; } = true;

        public string LastModifiedFieldAlias { get; set; } = "lastModifiedDate";
        public string ChangeFrequencyFieldAlias { get; set; } = "changeFrequency";
        public string PriorityFieldAlias { get; set; } = "priority";

        public string ReturnContentType { get; set; } = "application/xml";
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}
