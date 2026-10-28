using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Dashboard
{
    /// <summary>
    /// Whether robots.txt lets search engines crawl the site: not indexable when the "User-agent: *" group has "Disallow: /".
    /// </summary>
    public class RobotsTxtIndexableDataSource : IDashboardDataSource
    {
        private readonly IRobotsTxtService _robotsTxtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RobotsTxtIndexableDataSource(IRobotsTxtService robotsTxtService, IHttpContextAccessor httpContextAccessor)
        {
            _robotsTxtService = robotsTxtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public string Alias => "robotsTxt.indexable";

        public Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters)
        {
            // Includes the sitemap lines added when robots.txt is served, so the tile matches what crawlers get.
            var request = _httpContextAccessor.HttpContext?.Request;
            var content = request != null ? _robotsTxtService.GetContentWithSitemaps(request) : _robotsTxtService.GetContent();

            return Task.FromResult(Create(content));
        }

        public static DashboardData Create(string content)
        {
            var inWildcardGroup = false;
            var blocksAll = false;
            var rules = 0;
            var sitemaps = new List<string>();

            foreach (var rawLine in (content ?? string.Empty).Split('\n'))
            {
                var line = rawLine.Split('#')[0].Trim();
                var separator = line.IndexOf(':');
                if (separator < 0)
                    continue;

                var key = line[..separator].Trim().ToLowerInvariant();
                var value = line[(separator + 1)..].Trim();
                switch (key)
                {
                    case "user-agent":
                        inWildcardGroup = value == "*";
                        break;
                    case "disallow":
                        rules++;
                        if (inWildcardGroup && value == "/")
                            blocksAll = true;
                        break;
                    case "allow":
                        rules++;
                        break;
                    case "sitemap":
                        sitemaps.Add(value);
                        break;
                }
            }

            return new DashboardData
            {
                Status = blocksAll ? DashboardDataStatus.Danger : DashboardDataStatus.Positive,
                Text = blocksAll ? "No" : "Yes",
                Description = blocksAll ? "robots.txt blocks all crawlers" : "robots.txt allows crawling",
                Items =
                {
                    new DashboardDataItem { Label = "Rules", Value = rules },
                    new DashboardDataItem
                    {
                        Label = "Sitemap reference",
                        Text = sitemaps.Count > 0 ? string.Join(", ", sitemaps) : "missing",
                        Status = sitemaps.Count > 0 ? DashboardDataStatus.None : DashboardDataStatus.Warning
                    }
                }
            };
        }
    }
}
