using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
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
