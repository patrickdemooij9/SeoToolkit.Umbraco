using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class CoreWebVitalsCheck : ISiteCheck
    {
        private const int WarningThresholdMs = 2500;
        private const int ErrorThresholdMs = 4000;

        public string Name => "Core Web Vitals Signal Check";
        public string Alias => "CoreWebVitalsCheck";
        public string Description => "Checks response time as a Core Web Vitals performance signal";
        public string ErrorMessage => "Some pages are likely to perform poorly.";

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
                return $"Performance signal: {rating} ({responseTime} ms response time).";
            }

            return "Performance signal indicates potential Core Web Vitals issues.";
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
