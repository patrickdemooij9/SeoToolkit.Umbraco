using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Redirects.Core.Dashboard
{
    /// <summary>
    /// Which enabled redirects are still used, based on when each was last hit.
    /// Parameters: "recentDays" (default 30), "staleDays" (default 90), "take" (default 5).
    /// </summary>
    public class RedirectsUsageDataSource : IDashboardDataSource
    {
        private readonly IRedirectsRepository _redirectsRepository;
        private readonly ILanguageService _languageService;

        public RedirectsUsageDataSource(IRedirectsRepository redirectsRepository, ILanguageService languageService)
        {
            _redirectsRepository = redirectsRepository;
            _languageService = languageService;
        }

        public string Alias => "redirects.usage";

        public async Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters)
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
            // Redirects to a node can target a specific culture, which is needed to build the right URL
            var languages = mostRecentlyHit.Any(it => it.NewNodeCultureId.HasValue)
                ? (await _languageService.GetAllAsync()).ToArray()
                : Array.Empty<ILanguage>();
            data.Items.AddRange(mostRecentlyHit.Select(it => new DashboardDataItem
            {
                Label = $"{it.OldUrl} → {it.GetNewUrl(languages.FirstOrDefault(l => l.Id == it.NewNodeCultureId)?.IsoCode)}",
                Date = it.LastHit
            }));

            return data;
        }

        private static int GetInt(IReadOnlyDictionary<string, string> parameters, string key, int fallback)
        {
            return parameters.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : fallback;
        }
    }
}
