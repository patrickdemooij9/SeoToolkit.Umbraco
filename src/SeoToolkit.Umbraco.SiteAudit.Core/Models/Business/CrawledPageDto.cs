using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class CrawledPageDto
    {
        public CrawledPageDto ParentPage { get; set; }
        public Uri PageUrl { get; set; }
        public int StatusCode { get; set; }
        public List<PageCrawlResult> Results { get; }

        public CrawledPageDto()
        {
            Results = new List<PageCrawlResult>();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CrawledPageDto page))
                return false;
            return page.PageUrl.Equals(PageUrl);
        }
    }
}
