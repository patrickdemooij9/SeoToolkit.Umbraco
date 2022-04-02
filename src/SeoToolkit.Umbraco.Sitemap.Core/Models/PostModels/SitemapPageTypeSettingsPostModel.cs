namespace SeoToolkit.Umbraco.Sitemap.Core.Models.PostModels
{
    public class SitemapPageTypeSettingsPostModel
    {
        public int ContentTypeId { get; set; }
        public bool HideFromSitemap { get; set; }
        public string ChangeFrequency { get; set; }
        public double? Priority { get; set; }
    }
}
