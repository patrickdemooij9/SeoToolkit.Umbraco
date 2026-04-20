using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class SchemaEditorValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            return GetGuids(value);
        }

        public object ConvertObjectToEditorValue(object value)
        {
            var guids = GetGuids(value);
            return new SchemaEditorValueModel
            {
                Schemas = guids.Select(g => g.ToString()).ToArray()
            };
        }

        public object ConvertDatabaseToObject(object value)
        {
            return GetGuids(value);
        }

        public bool IsEmpty(object value)
        {
            return GetGuids(value).Length == 0;
        }

        private static Guid[] GetGuids(object value)
        {
            if (value is null)
                return [];

            if (value is Guid[] guidArray)
                return guidArray;

            if (value is JArray jArray)
                return ParseGuidsFromJArray(jArray);

            if (value is JObject jObj)
            {
                if (jObj["schemas"] is JArray schemasArr)
                    return ParseGuidsFromJArray(schemasArr);
                return [];
            }

            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Array)
                    return ParseGuidsFromJsonArray(jsonElement);

                if (jsonElement.ValueKind == JsonValueKind.Object &&
                    jsonElement.TryGetProperty("schemas", out var schemasEl) &&
                    schemasEl.ValueKind == JsonValueKind.Array)
                    return ParseGuidsFromJsonArray(schemasEl);

                return [];
            }

            var str = value.ToString();
            if (string.IsNullOrWhiteSpace(str))
                return [];

            try
            {
                var token = JToken.Parse(str);
                if (token is JArray arr)
                    return ParseGuidsFromJArray(arr);
                if (token is JObject obj && obj["schemas"] is JArray schemasArrFromStr)
                    return ParseGuidsFromJArray(schemasArrFromStr);
            }
            catch
            {
                // ignore
            }

            return [];
        }

        private static Guid[] ParseGuidsFromJArray(JArray arr)
        {
            var result = new List<Guid>();
            foreach (var token in arr)
            {
                if (token.Type == JTokenType.String && Guid.TryParse(token.Value<string>(), out var g))
                    result.Add(g);
            }
            return result.ToArray();
        }

        private static Guid[] ParseGuidsFromJsonArray(JsonElement element)
        {
            var result = new List<Guid>();
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && Guid.TryParse(item.GetString(), out var g))
                    result.Add(g);
            }
            return result.ToArray();
        }
    }
}
