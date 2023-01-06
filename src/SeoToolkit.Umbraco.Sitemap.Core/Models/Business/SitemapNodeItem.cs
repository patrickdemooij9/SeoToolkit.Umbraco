using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    public class SitemapNodeItem
    {
        public bool HideFromSitemap { get; set; }

        public string Url { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string ChangeFrequency { get; set; }
        public double? Priority { get; set; }
        public IPublishedContent Content { get; set; }

        public List<SitemapNodeAlternatePage> AlternatePages { get; set; }

        public SitemapNodeItem(string url)
        {
            Url = url;
            AlternatePages = new List<SitemapNodeAlternatePage>();
        }
    }
}
