using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.EventArgs
{
    public class PageCrawlCompleteArgs
    {
        public CrawledPageModel Page { get; set; }
        public SiteAuditContext Context { get; set; }
        public int TotalPagesFound { get; set; }
    }
}
