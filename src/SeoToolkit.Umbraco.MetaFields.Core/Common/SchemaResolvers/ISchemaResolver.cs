using Schema.NET;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public interface ISchemaResolver
    {
        SchemaProperty[] Properties { get; }

        IThing ToSchema(Dictionary<string, string> values);
    }
}
