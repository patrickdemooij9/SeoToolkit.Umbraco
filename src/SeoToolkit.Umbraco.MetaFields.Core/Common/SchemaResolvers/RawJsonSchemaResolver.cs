using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System.Collections.Generic;
using System.Text.Json;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class RawJsonSchemaResolver : ISchemaResolver
    {
        public string Name => "Raw JSON";
        public string Alias => "rawJson";

        public SchemaProperty[] Properties =>
            [
                new("json", "JSON", "Umb.PropertyEditorUi.TextArea", allowReference: false, valueConverter: new TextValueConverter())
            ];

        public IThing ToSchema(Dictionary<string, string> values)
        {
            var json = values.GetValueOrDefault("json") ?? string.Empty;

            // Validate that the value is a JSON object or array before rendering
            json = json.Trim();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object && doc.RootElement.ValueKind != JsonValueKind.Array)
                return null;

            return new RawJsonThing(json);
        }
    }
}
