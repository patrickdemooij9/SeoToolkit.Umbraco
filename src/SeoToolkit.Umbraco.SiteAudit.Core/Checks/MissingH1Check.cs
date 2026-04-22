using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class MissingH1Check : ISiteCheck
    {
        public string Name => "Missing H1 Check";
        public string Alias => "MissingH1Check";
        public string Description => "Checks if a page has a valid H1 heading";
        public string ErrorMessage => "Your site has invalid H1 headings!";

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.Content == null)
                yield break;

            var h1Nodes = page.Content.DocumentNode.SelectNodes("//h1");
            if (h1Nodes is null || h1Nodes.Count == 0)
            {
                yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Error };
                yield break;
            }

            if (h1Nodes.All(it => string.IsNullOrWhiteSpace(it.InnerText)))
            {
                yield return new CheckPageCrawlResult
                {
                    Result = SiteCrawlResultType.Error,
                    ExtraValues = new Dictionary<string, string> { { "IsEmpty", string.Empty } }
                };
            }
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return crawlResult.ExtraValues?.ContainsKey("IsEmpty") is true ? "Empty H1 tag found!" : "No H1 tag found!";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues?.Count == otherResult.ExtraValues?.Count;
        }
    }
}
