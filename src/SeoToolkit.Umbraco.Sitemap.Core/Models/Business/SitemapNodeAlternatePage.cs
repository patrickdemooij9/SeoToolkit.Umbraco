namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    public class SitemapNodeAlternatePage
    {
        public string Url { get; set; }
        public string Culture { get; set; }

        public SitemapNodeAlternatePage(string url, string culture)
        {
            Url = url;
            Culture = culture;
        }
    }
}
