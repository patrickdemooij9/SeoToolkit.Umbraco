using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class SiteAuditDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SiteAuditStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Uri StartingUrl { get; set; }
        public int? MaxPagesToCrawl { get; set; }
        public int TotalPagesFound { get; set; }
        public int DelayBetweenRequests { get; set; }
        public List<SiteCheckDto> SiteChecks { get; set; }
        public ConcurrentQueue<CrawledPageDto> CrawledPages { get; set; }

        public SiteAuditDto()
        {
            SiteChecks = new List<SiteCheckDto>();
            CrawledPages = new ConcurrentQueue<CrawledPageDto>();
        }

        public void AddPage(CrawledPageDto page)
        {
            if (CrawledPages.Contains(page))
                throw new ApplicationException($"Page with url {page.PageUrl} is already crawled!");
            CrawledPages.Enqueue(page);
        }
    }
}
