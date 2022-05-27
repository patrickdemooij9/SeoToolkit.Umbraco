using SeoToolkit.Umbraco.Common.Core.Interfaces;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class SeoDisplayCollectionBuilder : WeightedCollectionBuilderBase<SeoDisplayCollectionBuilder, SeoDisplayCollection, ISeoDisplayProvider>
    {
        protected override SeoDisplayCollectionBuilder This => this;
    }
}
