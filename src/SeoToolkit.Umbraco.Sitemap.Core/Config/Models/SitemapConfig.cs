using System;

namespace SeoToolkit.Umbraco.Sitemap.Core.Config.Models
{
    public class SitemapConfig
    {
        //If alternate pages is turned on, it'll show the alternate pages for the different languages.
        public bool ShowAlternatePages { get; set; } = true;

        public string LastModifiedFieldAlias { get; set; } = "lastModifiedDate";
        public string ChangeFrequencyFieldAlias { get; set; } = "changeFrequency";
        public string PriorityFieldAlias { get; set; } = "priority";

        public string ReturnContentType { get; set; } = "application/xml";
        public string LastModifiedFormat { get; set; } = "yyyy-MM-ddTHH:mm:sszzz";
        public string[] DisabledModules { get; set; } = Array.Empty<string>();

        public StructureMode StructureMode { get; set; } = StructureMode.MultiRoot;

        public string SitemapIndexPath { get; set; } = string.Empty;

        //Maximum amount of urls in a single sitemap. Collections with more urls are split over multiple sitemaps. The sitemap protocol allows at most 50000.
        //Defaults to int.MaxValue (no splitting) to stay backwards compatible. The default will change to 50000 in a future major version.
        public int MaxUrlsPerSitemap { get; set; } = int.MaxValue;

        //How long generated sitemap items are cached. Content changes clear the cache. Use 0 to disable caching.
        public int CacheDurationInMinutes { get; set; } = 60;

        //Aliases of sitemap collections that should not be rendered, for example "pages".
        public string[] DisabledCollections { get; set; } = Array.Empty<string>();
    }
}
