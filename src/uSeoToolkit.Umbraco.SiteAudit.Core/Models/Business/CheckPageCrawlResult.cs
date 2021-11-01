using System.Collections.Generic;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business
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
