using SeoToolkit.Umbraco.Common.Core.Collections;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Common.Core.Extensions
{
    public static class DashboardDataSourceCollectionExtensions
    {
        public static DashboardDataSourceCollectionBuilder DashboardDataSources(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<DashboardDataSourceCollectionBuilder>();
    }
}
