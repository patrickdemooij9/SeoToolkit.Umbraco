using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SchemaResolverCollection : BuilderCollectionBase<ISchemaResolver>
    {
        public SchemaResolverCollection(Func<IEnumerable<ISchemaResolver>> items) : base(items)
        {
        }
    }
}
