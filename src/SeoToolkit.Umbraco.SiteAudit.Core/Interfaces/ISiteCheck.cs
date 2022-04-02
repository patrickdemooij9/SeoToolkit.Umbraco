using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteCheck
    {
        string Name { get; }
        string Alias { get; }
        string Description { get; }
        string ErrorMessage { get; }

        IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page);
        string FormatMessage(CheckPageCrawlResult crawlResult);
        bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult);
    }
}
