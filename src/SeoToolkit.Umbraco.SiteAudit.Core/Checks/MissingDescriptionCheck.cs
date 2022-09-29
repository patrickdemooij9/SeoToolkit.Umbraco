using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class MissingDescriptionCheck : ISiteCheck
    {
        public string Name => "Missing Description Check";
        public string Alias => "MissingDescriptionCheck";
        public string Description => "Checks if you are missing any descriptions";
        public string ErrorMessage => "Your site has invalid descriptions!";
        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.Content == null) yield break;

            var descriptionTag = page.Content.DocumentNode.SelectSingleNode("//head/meta[@name='description']");
            if (descriptionTag is null)
            {
                yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Error };
                yield break;
            }

            var content = descriptionTag.GetAttributeValue<string>("content", string.Empty);
            if (string.IsNullOrWhiteSpace(content))
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
            return crawlResult.ExtraValues?.ContainsKey("IsEmpty") is true ? "Empty description tag!" : "No description tag found!";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues?.Count == otherResult.ExtraValues?.Count;
        }
    }
}
