using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class MissingTitleCheck : ISiteCheck
    {
        public string Name => "Missing Title Check";
        public string Alias => "MissingTitleCheck";
        public string Description => "Checks if you are missing any title tags";
        public string ErrorMessage => "Your site has invalid titles!";
        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.Content == null) yield break;

            var titleTag = page.Content.DocumentNode.SelectSingleNode("//head/title");
            if (titleTag is null)
            {
                yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Error };
                yield break;
            }

            if (string.IsNullOrWhiteSpace(titleTag.InnerText))
            {
                yield return new CheckPageCrawlResult
                {
                    Result = SiteCrawlResultType.Error,
                    ExtraValues = new Dictionary<string, string> {{"IsEmpty", ""}}
                };
            }
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return crawlResult.ExtraValues?.ContainsKey("IsEmpty") is true ? "Empty title tag!" : "No title tag found!";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues?.Count == otherResult.ExtraValues?.Count;
        }
    }
}
