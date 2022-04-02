using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Interfaces;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class DisplayCollectionBuilder : WeightedCollectionBuilderBase<DisplayCollectionBuilder, DisplayCollection, IDisplayProvider>
    {
        protected override DisplayCollectionBuilder This => this;
    }
}
