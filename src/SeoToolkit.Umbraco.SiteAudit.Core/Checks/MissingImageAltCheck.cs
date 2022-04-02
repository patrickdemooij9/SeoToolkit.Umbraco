using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class MissingImageAltCheck : ISiteCheck
    {
        public string Name => "Missing Image Alt Check";
        public string Alias => "MissingImageAltCheck";
        public string Description => "Checks if you have any images without an alt text";
        public string ErrorMessage => "There are images without an alt text!";
        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page)
        {
            var images = page.Content?.DocumentNode.SelectNodes("//img");
            if (images is null)
                yield break;

            foreach (var image in images)
            {
                if (!image.Attributes.Contains("alt") || string.IsNullOrWhiteSpace(image.Attributes["alt"].Value))
                {
                    yield return new CheckPageCrawlResult
                    {
                        Result = SiteCrawlResultType.Warning,
                        ExtraValues = new Dictionary<string, string> {{"ImageUrl", image.Attributes["src"]?.Value}}
                    };
                }
            }
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return $"No alt text found for image: {crawlResult.ExtraValues["ImageUrl"]}";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues["ImageUrl"] == otherResult.ExtraValues["ImageUrl"];
        }
    }
}
