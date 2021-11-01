using System.Collections.Generic;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class PageCrawlResult
    {
        public SiteCheckDto Check { get; set; }
        public SiteCrawlResultType Result { get; set; }
        public Dictionary<string, string> ExtraValues { get; set; }

        public PageCrawlResult()
        {
            ExtraValues = new Dictionary<string, string>();
        }

        public PageCrawlResult(SiteCheckDto siteCheck, CheckPageCrawlResult crawlResult)
        {
            Check = siteCheck;
            Result = crawlResult.Result;
            ExtraValues = crawlResult.ExtraValues;
        }
    }
}
