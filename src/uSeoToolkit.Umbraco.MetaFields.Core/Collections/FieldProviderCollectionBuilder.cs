using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class FieldProviderCollectionBuilder : WeightedCollectionBuilderBase<FieldProviderCollectionBuilder, FieldProviderCollection, IFieldProvider>
    {
        protected override FieldProviderCollectionBuilder This => this;
    }
}
