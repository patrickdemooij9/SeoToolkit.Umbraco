using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class CheckPageCrawlResult
    {
        public SiteCrawlResultType Result { get; set; }
        public Dictionary<string, string> ExtraValues { get; set; }

        public CheckPageCrawlResult()
        {
            ExtraValues = new Dictionary<string, string>();
        }
    }
}
