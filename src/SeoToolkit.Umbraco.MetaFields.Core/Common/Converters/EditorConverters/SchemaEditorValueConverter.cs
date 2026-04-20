using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using System;
using System.Linq;
using System.Text.Json;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class SchemaEditorValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            return GetSchemas(value);
        }

        public object ConvertObjectToEditorValue(object value)
        {
            return new SchemaEditorValueModel
            {
                Schemas = GetSchemas(value)
            };
        }

        public object ConvertDatabaseToObject(object value)
        {
            return GetSchemas(value);
        }

        public bool IsEmpty(object value)
        {
            return GetSchemas(value).Any() == false;
        }

        private static SchemaEditorModel[] GetSchemas(object value)
        {
            if (value is null)
                return [];

            if (value is SchemaEditorModel[] schemaArray)
                return schemaArray;

            if (value is SchemaEditorValueModel model)
                return model.Schemas ?? [];

            if (value is JObject jObject)
            {
                if (jObject["schemas"] is JToken schemasToken)
                    return schemasToken.ToObject<SchemaEditorModel[]>() ?? [];

                return jObject.ToObject<SchemaEditorModel[]>() ?? [];
            }

            if (value is JArray jArray)
                return jArray.ToObject<SchemaEditorModel[]>() ?? [];

            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty("schemas", out var schemasProperty))
                    return JsonSerializer.Deserialize<SchemaEditorModel[]>(schemasProperty.GetRawText()) ?? [];

                if (jsonElement.ValueKind == JsonValueKind.Array)
                    return JsonSerializer.Deserialize<SchemaEditorModel[]>(jsonElement.GetRawText()) ?? [];
            }

            var valueString = value.ToString();
            if (string.IsNullOrWhiteSpace(valueString))
                return [];

            try
            {
                var wrapper = JsonSerializer.Deserialize<SchemaEditorValueModel>(valueString);
                if (wrapper?.Schemas != null)
                    return wrapper.Schemas;

                var schemas = JsonSerializer.Deserialize<SchemaEditorModel[]>(valueString);
                return schemas ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}
