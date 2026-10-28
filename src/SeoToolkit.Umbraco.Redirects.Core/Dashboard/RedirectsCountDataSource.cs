using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;

namespace SeoToolkit.Umbraco.Redirects.Core.Dashboard
{
    /// <summary>
    /// The number of redirects, split by type.
    /// </summary>
    public class RedirectsCountDataSource : IDashboardDataSource
    {
        private readonly IRedirectsRepository _redirectsRepository;

        public RedirectsCountDataSource(IRedirectsRepository redirectsRepository)
        {
            _redirectsRepository = redirectsRepository;
        }

        public string Alias => "redirects.count";

        public Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters)
        {
            var statistics = _redirectsRepository.GetStatistics(DateTime.UtcNow, DateTime.UtcNow);

            return Task.FromResult(new DashboardData
            {
                Status = DashboardDataStatus.None,
                Value = statistics.Total,
                Description = $"{statistics.Enabled} enabled",
                Items =
                {
                    new DashboardDataItem { Label = "Permanent (301)", Value = statistics.Permanent },
                    new DashboardDataItem { Label = "Temporary / other", Value = statistics.Total - statistics.Permanent },
                    new DashboardDataItem { Label = "Disabled", Value = statistics.Total - statistics.Enabled }
                }
            });
        }
    }
}
