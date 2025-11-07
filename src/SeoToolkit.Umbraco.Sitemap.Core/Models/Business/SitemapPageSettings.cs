using System;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    public class SitemapPageSettings
    {
        [Obsolete("Use ContentTypeGuid instead")]
        public int ContentTypeId { get; set; }
        public Guid ContentTypeGuid { get; set; }
        public bool HideFromSitemap { get; set; }
        public string ChangeFrequency { get; set; }
        public double? Priority { get; set; }
    }
}
