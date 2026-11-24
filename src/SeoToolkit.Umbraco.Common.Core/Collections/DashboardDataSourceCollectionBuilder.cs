using SeoToolkit.Umbraco.Common.Core.Interfaces;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class DashboardDataSourceCollectionBuilder : WeightedCollectionBuilderBase<DashboardDataSourceCollectionBuilder, DashboardDataSourceCollection, IDashboardDataSource>
    {
        protected override DashboardDataSourceCollectionBuilder This => this;
    }
}
