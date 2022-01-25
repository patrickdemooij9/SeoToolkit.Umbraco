using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.Common.Core.Interfaces;

namespace uSeoToolkit.Umbraco.Common.Core.Collections
{
    public class DisplayCollectionBuilder : WeightedCollectionBuilderBase<DisplayCollectionBuilder, DisplayCollection, IDisplayProvider>
    {
        protected override DisplayCollectionBuilder This => this;
    }
}
