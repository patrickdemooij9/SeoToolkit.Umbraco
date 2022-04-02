using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class FieldProviderCollectionBuilder : WeightedCollectionBuilderBase<FieldProviderCollectionBuilder, FieldProviderCollection, IFieldProvider>
    {
        protected override FieldProviderCollectionBuilder This => this;
    }
}
