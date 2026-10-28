using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Dashboard
{
    /// <summary>
    /// Whether robots.txt lets search engines crawl the site, based on the same validation the robots.txt editor shows.
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

            return Task.FromResult(Create(content, _robotsTxtService.Validate(content)));
        }

        public static DashboardData Create(string content, IEnumerable<RobotsTxtValidation> validations)
        {
            var issues = validations.ToArray();
            var blocksAll = issues.Any(IsBlockingAll);
            var hasSitemap = (content ?? string.Empty)
                .Split('\n')
                .Any(line => line.TrimStart().StartsWith("sitemap:", StringComparison.InvariantCultureIgnoreCase));

            var data = new DashboardData
            {
                Status = blocksAll ? DashboardDataStatus.Danger : issues.Length > 0 ? DashboardDataStatus.Warning : DashboardDataStatus.Positive,
                Text = blocksAll ? "No" : "Yes",
                Description = blocksAll ? "robots.txt blocks all crawlers"
                    : issues.Length > 0 ? $"robots.txt allows crawling, {issues.Length} issue(s)"
                    : "robots.txt allows crawling"
            };
            data.Items.AddRange(issues.Select(issue => new DashboardDataItem
            {
                Label = $"Line {issue.LineNumber}",
                Text = issue.Error,
                Status = IsBlockingAll(issue) ? DashboardDataStatus.Danger : DashboardDataStatus.Warning
            }));
            data.Items.Add(new DashboardDataItem
            {
                Label = "Sitemap reference",
                Text = hasSitemap ? "present" : "missing",
                Status = hasSitemap ? DashboardDataStatus.None : DashboardDataStatus.Warning
            });

            return data;
        }

        private static bool IsBlockingAll(RobotsTxtValidation validation) =>
            validation.Error == DefaultRobotsTxtValidator.DisallowAllWarning;
    }
}
