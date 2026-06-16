using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class MissingCanonicalCheck : ISiteCheck
    {
        public string Name => "Missing Canonical Check";
        public string Alias => "MissingCanonicalCheck";
        public string Description => "Checks if a page has a valid canonical tag";
        public string ErrorMessage => "Your site has invalid canonical tags!";

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.Content == null)
                yield break;

            var canonicalTag = page.Content.DocumentNode.SelectSingleNode("//head/link[translate(@rel,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='canonical']");
            if (canonicalTag is null)
            {
                yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Warning };
                yield break;
            }

            var href = canonicalTag.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrWhiteSpace(href))
            {
                yield return new CheckPageCrawlResult
                {
                    Result = SiteCrawlResultType.Warning,
                    ExtraValues = new Dictionary<string, string> { { "IsEmpty", string.Empty } }
                };
            }
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return crawlResult.ExtraValues?.ContainsKey("IsEmpty") is true ? "Canonical tag is empty!" : "No canonical tag found!";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues?.Count == otherResult.ExtraValues?.Count;
        }
    }
}
