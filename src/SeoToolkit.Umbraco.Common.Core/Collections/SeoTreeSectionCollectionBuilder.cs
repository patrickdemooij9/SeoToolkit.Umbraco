using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class SeoTreeSectionCollectionBuilder : WeightedCollectionBuilderBase<SeoTreeSectionCollectionBuilder, SeoTreeSectionCollection, ISeoTreeSection>
    {
        protected override SeoTreeSectionCollectionBuilder This => this;
    }
}
