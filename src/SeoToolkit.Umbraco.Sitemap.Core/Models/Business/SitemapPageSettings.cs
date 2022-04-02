namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    public class SitemapPageSettings
    {
        public int ContentTypeId { get; set; }
        public bool HideFromSitemap { get; set; }
        public string ChangeFrequency { get; set; }
        public double? Priority { get; set; }
    }
}
