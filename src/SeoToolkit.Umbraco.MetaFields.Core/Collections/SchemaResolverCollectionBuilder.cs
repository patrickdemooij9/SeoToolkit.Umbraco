using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SchemaResolverCollectionBuilder : WeightedCollectionBuilderBase<SchemaResolverCollectionBuilder, SchemaResolverCollection, ISchemaResolver>
    {
        protected override SchemaResolverCollectionBuilder This => this;
    }
}
