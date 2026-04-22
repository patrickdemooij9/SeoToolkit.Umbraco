using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class OrphanedPageCheck : ISiteCheck
    {
        public string Name => "Orphaned Page Risk Check";
        public string Alias => "OrphanedPageCheck";
        public string Description => "Checks for pages without internal links to other pages";
        public string ErrorMessage => "Some pages may become orphaned because they are not internally linked.";

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.Content == null)
                yield break;

            var links = page.Content.DocumentNode.SelectNodes("//a[@href]");
            if (links is null || links.Count == 0)
            {
                yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Warning };
                yield break;
            }

            var internalLinkCount = 0;
            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrWhiteSpace(href))
                    continue;

                if (href.StartsWith("#", StringComparison.InvariantCultureIgnoreCase) ||
                    href.StartsWith("mailto:", StringComparison.InvariantCultureIgnoreCase) ||
                    href.StartsWith("tel:", StringComparison.InvariantCultureIgnoreCase) ||
                    href.StartsWith("javascript:", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
                {
                    if (string.Equals(absoluteUri.Authority, page.Url.Authority, StringComparison.InvariantCultureIgnoreCase) && !IsSamePage(page.Url, absoluteUri))
                        internalLinkCount++;
                }
                else if (Uri.TryCreate(page.Url, href, out var relativeUri))
                {
                    if (!IsSamePage(page.Url, relativeUri))
                        internalLinkCount++;
                }
            }

            if (internalLinkCount == 0)
                yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Warning };
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return "No internal links found on this page. This can increase orphan-page risk.";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return true;
        }

        private bool IsSamePage(Uri source, Uri target)
        {
            return source.GetLeftPart(UriPartial.Path).Equals(target.GetLeftPart(UriPartial.Path), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
