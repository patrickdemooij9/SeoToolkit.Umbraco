using System.Collections.Generic;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Models.Business;

namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    /// <summary>
    /// Provides the data for one or more tiles on the SEO dashboard. Tiles reference a data source by its alias
    /// and can pass parameters, so one data source can serve several tiles.
    /// </summary>
    public interface IDashboardDataSource
    {
        string Alias { get; }

        Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters);
    }
}
