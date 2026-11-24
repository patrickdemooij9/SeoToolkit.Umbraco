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
    /// The score of the latest finished site audit, with the number of failing pages per check.
    /// </summary>
    public class SiteAuditHealthDataSource : IDashboardDataSource
    {
        private readonly SiteAuditService _siteAuditService;

        public SiteAuditHealthDataSource(SiteAuditService siteAuditService)
        {
            _siteAuditService = siteAuditService;
        }

        public string Alias => "siteAudit.health";

        public Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters)
        {
            return Task.FromResult(Create(_siteAuditService.GetLatestFinishedSummary()));
        }

        public static DashboardData Create(SiteAuditResultsSummary summary)
        {
            var score = summary?.GetScore();
            if (score is null)
            {
                return new DashboardData
                {
                    Status = DashboardDataStatus.Warning,
                    Description = "Run a site audit to get a score"
                };
            }

            var data = new DashboardData
            {
                Status = score >= 80 ? DashboardDataStatus.Positive : score >= 60 ? DashboardDataStatus.Warning : DashboardDataStatus.Danger,
                Value = score,
                Total = 100,
                Description = $"{summary.Pages.Count} pages",
                Date = summary.CreatedDate
            };
            data.Items.AddRange(summary.Checks.Select(check =>
            {
                var results = summary.Pages.SelectMany(page => page.Results.Where(it => it.CheckId == check.Id)).ToArray();
                return new DashboardDataItem
                {
                    Label = check.Check.Name,
                    Value = summary.GetPagesFailing(check.Id).Count,
                    Status = results.Any(it => it.Result == SiteCrawlResultType.Error) ? DashboardDataStatus.Danger
                        : results.Any(it => it.Result == SiteCrawlResultType.Warning) ? DashboardDataStatus.Warning
                        : DashboardDataStatus.Positive
                };
            }));
            return data;
        }
    }
}
