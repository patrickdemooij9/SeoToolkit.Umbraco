using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Checks
{
    public class BrokenLinkCheck : ISiteCheck
    {
        private readonly ILogger<BrokenLinkCheck> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string BrokenLinkUrl = "BrokenLinkUrl";
        private const string BrokenLinkStatusCode = "BrokenLinkStatusCode";
        
        public string Name => "Broken Link Check";
        public string Alias => "BrokenLinkCheck";

        public string Description => "Checks for broken links on your page!";
        public string ErrorMessage => "Your site has some broken links!";

        public BrokenLinkCheck(ILogger<BrokenLinkCheck> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public string FormatMessage(CheckPageCrawlResult crawlResult)
        {
            return $"Broken url: {crawlResult.ExtraValues[BrokenLinkUrl]} ({crawlResult.ExtraValues[BrokenLinkStatusCode]})";
        }

        public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page)
        {
            if (page.FoundUrls?.Any() != true)
                return new List<CheckPageCrawlResult>(0);
            
            var results = new List<CheckPageCrawlResult>();
            var httpClient = _httpClientFactory.CreateClient("BrokenLinkCheck");
            foreach (var url in page.FoundUrls)
            {
                using (var message = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    try
                    {
                        var response = httpClient.SendAsync(message, CancellationToken.None).Result;
                        if (!response.IsSuccessStatusCode)
                            results.Add(new CheckPageCrawlResult
                            {
                                Result = SiteCrawlResultType.Error,
                                ExtraValues = new Dictionary<string, string>
                                {
                                    { BrokenLinkUrl, url.ToString() },
                                    { BrokenLinkStatusCode, response.StatusCode.ToString() }
                                }
                            });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Something went wrong");
                    }
                }
            }

            return results;
        }

        public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult)
        {
            return result.ExtraValues[BrokenLinkUrl] == otherResult.ExtraValues[BrokenLinkUrl];
        }
    }
}
