using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Dashboard
{
    /// <summary>
    /// The pages of the latest finished site audit that fail one or more checks.
    /// Parameters: "checks" (comma-separated check aliases, e.g. "MissingTitleCheck,MissingDescriptionCheck") and "take" (default 5).
    /// </summary>
    public class SiteAuditChecksDataSource : IDashboardDataSource
    {
        private readonly SiteAuditService _siteAuditService;

        public SiteAuditChecksDataSource(SiteAuditService siteAuditService)
        {
            _siteAuditService = siteAuditService;
        }

        public string Alias => "siteAudit.checks";

        public Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters)
        {
            var aliases = parameters.TryGetValue("checks", out var checks)
                ? checks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();
            var take = parameters.TryGetValue("take", out var takeValue) && int.TryParse(takeValue, out var parsedTake) ? parsedTake : 5;

            return Task.FromResult(Create(_siteAuditService.GetLatestFinishedSummary(), aliases, take));
        }

        public static DashboardData Create(SiteAuditResultsSummary summary, string[] checkAliases, int take)
        {
            if (summary is null)
            {
                return new DashboardData
                {
                    Status = DashboardDataStatus.Warning,
                    Description = "Needs a site audit"
                };
            }

            var checks = summary.Checks
                .Where(it => checkAliases.Contains(it.Check.Alias, StringComparer.InvariantCultureIgnoreCase))
                .ToArray();
            var checkIds = checks.Select(it => it.Id).ToArray();
            var failingPages = summary.GetPagesFailing(checkIds);
            var failingResults = failingPages.SelectMany(page => page.Results.Where(it => checkIds.Contains(it.CheckId))).ToArray();

            var data = new DashboardData
            {
                Status = failingResults.Any(it => it.Result == SiteCrawlResultType.Error) ? DashboardDataStatus.Danger
                    : failingResults.Any(it => it.Result == SiteCrawlResultType.Warning) ? DashboardDataStatus.Warning
                    : DashboardDataStatus.Positive,
                Value = failingPages.Count,
                Total = summary.Pages.Count,
                Date = summary.CreatedDate
            };

            if (checks.Length > 1)
            {
                data.Items.AddRange(checks.Select(check => new DashboardDataItem
                {
                    Label = check.Check.Name,
                    Value = summary.GetPagesFailing(check.Id).Count
                }));
            }
            data.Items.AddRange(failingPages.Take(take).Select(page => new DashboardDataItem
            {
                Label = page.Url,
                Status = page.Results.Any(it => checkIds.Contains(it.CheckId) && it.Result == SiteCrawlResultType.Error)
                    ? DashboardDataStatus.Danger
                    : DashboardDataStatus.Warning
            }));

            return data;
        }
    }
}
