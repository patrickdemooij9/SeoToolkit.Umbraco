using Schema.NET;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public interface ISchemaResolver
    {
        string Name { get; }
        string Alias { get; }
        SchemaProperty[] Properties { get; }

        IThing ToSchema(Dictionary<string, string> values);
    }
}
