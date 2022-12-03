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
    public class BrokenLinkCheck : ISiteCheck
    {
        private readonly ILogger<BrokenLinkCheck> _logger;
        private readonly HttpClient _httpClient;

        private const string BrokenLinkUrl = "BrokenLinkUrl";
        private const string BrokenLinkStatusCode = "BrokenLinkStatusCode";
        
        public string Name => "Broken Link Check";
        public string Alias => "BrokenLinkCheck";

        public string Description => "Checks for broken links on your page!";
        public string ErrorMessage => "Your site has some broken links!";

        public BrokenLinkCheck(ILogger<BrokenLinkCheck> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return $"Broken url: {crawlResult.ExtraValues[BrokenLinkUrl]} ({crawlResult.ExtraValues[BrokenLinkStatusCode]})";
        }

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
        {
            if (page.FoundUrls?.Any() != true)
                return new List<CheckPageCrawlResult>(0);
            
            var results = new List<CheckPageCrawlResult>();
            foreach (var url in page.FoundUrls)
            {
                //Check if we have already visited this url before
                var cachedStatusCode = context.GetStatusCode(url);
                if (cachedStatusCode != null)
                {
                    if (cachedStatusCode < 200 || cachedStatusCode > 299)
                    {
                        results.Add(CreateResult(url.ToString(), cachedStatusCode.Value));
                    }
                    continue;
                }

                using (var message = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    try
                    {
                        var response = _httpClient.SendAsync(message, CancellationToken.None).Result;
                        if (!response.IsSuccessStatusCode)
                            results.Add(CreateResult(url.ToString(), (int)response.StatusCode));

                        context.AddUrlIfNotPresent(url, (int)response.StatusCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Something went wrong when trying to access {url}");
                    }
                }
            }

            return results;
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues[BrokenLinkUrl] == otherResult.ExtraValues[BrokenLinkUrl];
        }

        private CheckPageCrawlResult CreateResult(string url, int statusCode)
        {
            return new CheckPageCrawlResult
            {
                Result = SiteCrawlResultType.Error,
                ExtraValues = new Dictionary<string, string>
                                {
                                    { BrokenLinkUrl, url },
                                    { BrokenLinkStatusCode, statusCode.ToString() }
                                }
            };
        }
    }
}
