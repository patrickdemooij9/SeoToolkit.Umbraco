using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SeoGroupCollectionBuilder : OrderedCollectionBuilderBase<SeoGroupCollectionBuilder, SeoGroupCollection, ISeoFieldGroup>
    {
        protected override SeoGroupCollectionBuilder This => this;
    }
}
