using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SeoConverterCollectionBuilder : WeightedCollectionBuilderBase<SeoConverterCollectionBuilder, SeoConverterCollection, ISeoValueConverter>
    {
        protected override SeoConverterCollectionBuilder This => this;
    }
}
