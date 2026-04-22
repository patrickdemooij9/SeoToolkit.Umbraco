using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class ThinContentCheck : ISiteCheck
    {
        private const int MinimumRecommendedWordCount = 150;
        private const string WordCountKey = "WordCount";
        private const string MinimumWordCountKey = "MinimumWordCount";

        public string Name => "Thin Content Check";
        public string Alias => "ThinContentCheck";
        public string Description => "Checks for pages with limited body text content";
        public string ErrorMessage => "Some pages may have thin content!";

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.Content == null)
                yield break;

            var bodyNode = page.Content.DocumentNode.SelectSingleNode("//body");
            if (bodyNode is null)
                yield break;

            var cleanText = HtmlEntity.DeEntitize(bodyNode.InnerText ?? string.Empty);
            var wordCount = Regex.Matches(cleanText, @"\b[\p{L}\p{N}']+\b").Count;

            if (wordCount < MinimumRecommendedWordCount)
            {
                yield return new CheckPageCrawlResult
                {
                    Result = SiteCrawlResultType.Warning,
                    ExtraValues = new Dictionary<string, string>
                    {
                        { WordCountKey, wordCount.ToString() },
                        { MinimumWordCountKey, MinimumRecommendedWordCount.ToString() }
                    }
                };
                yield break;
            }
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            if (crawlResult.ExtraValues != null
                && crawlResult.ExtraValues.TryGetValue(WordCountKey, out var wordCount)
                && crawlResult.ExtraValues.TryGetValue(MinimumWordCountKey, out var minimumWordCount))
            {
                return $"Thin content detected: {wordCount} words (recommended at least {minimumWordCount}).";
            }

            return "Thin content detected.";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return true;
        }
    }
}
