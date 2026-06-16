using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class PagePerformanceCheck : ISiteCheck
    {
        private const int WarningThresholdMs = 2500;
        private const int ErrorThresholdMs = 4000;

        public string Name => "Page performance check";
        public string Alias => "PagePerformanceCheck";
        public string Description => "Checks the response time of your pages";
        public string ErrorMessage => "Some pages are taking a long time to respond.";

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.RequestStarted == default || page.RequestCompleted <= page.RequestStarted)
                yield break;

            var responseTime = (int)Math.Round((page.RequestCompleted - page.RequestStarted).TotalMilliseconds);
            if (responseTime < WarningThresholdMs)
                yield break;

            var isError = responseTime >= ErrorThresholdMs;
            yield return new CheckPageCrawlResult
            {
                Result = isError ? SiteCrawlResultType.Error : SiteCrawlResultType.Warning,
                ExtraValues = new Dictionary<string, string>
                {
                    { "ResponseTimeMs", responseTime.ToString() },
                    { "Rating", isError ? "Poor" : "Needs Improvement" }
                }
            };
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            if (crawlResult.ExtraValues != null
                && crawlResult.ExtraValues.TryGetValue("Rating", out var rating)
                && crawlResult.ExtraValues.TryGetValue("ResponseTimeMs", out var responseTime))
            {
                return $"Page performance: {rating} ({responseTime} ms response time).";
            }

            return "Your load time for this page is taking longer than the thresholds specified.";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues != null
                   && otherResult.ExtraValues != null
                   && result.ExtraValues.TryGetValue("Rating", out var resultRating)
                   && otherResult.ExtraValues.TryGetValue("Rating", out var otherResultRating)
                   && resultRating == otherResultRating;
        }
    }
}
