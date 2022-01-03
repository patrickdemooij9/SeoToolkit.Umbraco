namespace uSeoToolkit.Umbraco.Sitemap.Core.Config.Models
{
    public class SitemapAppSettingsModel
    {
        //If alternate pages is turned on, it'll show the alternate pages for the different languages.
        public bool ShowAlternatePages { get; set; } = true;

        public string LastModifiedFieldAlias { get; set; } = "lastModifiedDate";
        public string ChangeFrequencyFieldAlias { get; set; } = "changeFrequency";
        public string PriorityFieldAlias { get; set; } = "priority";
    }
}
