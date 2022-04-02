using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class BrokenImageCheck : ISiteCheck
    {
        private readonly ILogger<BrokenImageCheck> _logger;
        private readonly HttpClient _httpClient;

        private const string BrokenLinkUrl = "BrokenLinkUrl";
        private const string BrokenLinkStatusCode = "BrokenLinkStatusCode";

        public string Name => "Broken Image Check";
        public string Alias => "BrokenImageCheck";
        public string Description => "Checks if there are any missing images";
        public string ErrorMessage => "You have broken images on your site!";

        public BrokenImageCheck(ILogger<BrokenImageCheck> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page)
        {
            if (page.Content is null)
                return new List<CheckPageCrawlResult>(0);

            var images = page.Content.DocumentNode.SelectNodes("//img")?.Where(it => it.Attributes?.Contains("src") is true);
            if (images is null)
                return new List<CheckPageCrawlResult>(0);

            var results = new List<CheckPageCrawlResult>();
            foreach (var imageElement in images)
            {
                var sourceUrl = imageElement.Attributes["src"].Value;
                if (string.IsNullOrWhiteSpace(sourceUrl))
                    continue;

                if (!sourceUrl.StartsWith("http"))
                    sourceUrl = new Uri(page.Url, sourceUrl).ToString();
                using (var message = new HttpRequestMessage(HttpMethod.Head, sourceUrl))
                {
                    try
                    {
                        var response = _httpClient.SendAsync(message, CancellationToken.None).Result;
                        if (!response.IsSuccessStatusCode)
                            results.Add(new CheckPageCrawlResult
                            {
                                Result = SiteCrawlResultType.Error,
                                ExtraValues = new Dictionary<string, string>
                                {
                                    { BrokenLinkUrl, sourceUrl },
                                    { BrokenLinkStatusCode, response.StatusCode.ToString() }
                                }
                            });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Something went wrong!");
                    }
                }
            }

            return results;
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return $"Broken image url: {crawlResult.ExtraValues[BrokenLinkUrl]} ({crawlResult.ExtraValues[BrokenLinkStatusCode]})";
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues[BrokenLinkUrl] == otherResult.ExtraValues[BrokenLinkUrl];
        }
    }
}
