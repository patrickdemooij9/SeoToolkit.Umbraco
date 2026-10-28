using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;

namespace SeoToolkit.Umbraco.Redirects.Core.Dashboard
{
    /// <summary>
    /// Which enabled redirects are still used, based on when each was last hit.
    /// Parameters: "recentDays" (default 30), "staleDays" (default 90), "take" (default 5).
    /// </summary>
    public class RedirectsUsageDataSource : IDashboardDataSource
    {
        private readonly IRedirectsRepository _redirectsRepository;

        public RedirectsUsageDataSource(IRedirectsRepository redirectsRepository)
        {
            _redirectsRepository = redirectsRepository;
        }

        public string Alias => "redirects.usage";

        public Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters)
        {
            var recentDays = GetInt(parameters, "recentDays", 30);
            var staleDays = GetInt(parameters, "staleDays", 90);
            var take = GetInt(parameters, "take", 5);

            var now = DateTime.UtcNow;
            var statistics = _redirectsRepository.GetStatistics(now.AddDays(-recentDays), now.AddDays(-staleDays));
            var mostRecentlyHit = _redirectsRepository.GetMostRecentlyHit(take);

            var data = new DashboardData
            {
                Status = statistics.Stale + statistics.NeverHit > 0 ? DashboardDataStatus.Warning : DashboardDataStatus.Positive,
                Value = statistics.RecentlyHit,
                Total = statistics.Enabled,
                Items =
                {
                    new DashboardDataItem { Label = $"No hit for {staleDays}+ days", Value = statistics.Stale },
                    new DashboardDataItem { Label = "No hit recorded", Value = statistics.NeverHit }
                }
            };
            data.Items.AddRange(mostRecentlyHit.Select(it => new DashboardDataItem
            {
                Label = $"{it.OldUrl} → {it.GetNewUrl()}",
                Date = it.LastHit
            }));

            return Task.FromResult(data);
        }

        private static int GetInt(IReadOnlyDictionary<string, string> parameters, string key, int fallback)
        {
            return parameters.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : fallback;
        }
    }
}
