using Newtonsoft.Json;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class ListValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            return value?.ToString();
        }

        public object ConvertObjectToEditorValue(object value)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return Array.Empty<string>();
            }
            if (value is string[] values)
            {
                return values;
            }
            return new string[] { value?.ToString() };
        }

        public object ConvertDatabaseToObject(object value)
        {
            // Always return a non-null array so null and empty-string both export as "[]" and
            // converge on deploy (a null result would export as an omitted, never-matching field).
            var raw = value?.ToString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<string>();
            }

            return JsonConvert.DeserializeObject<string[]>(raw) ?? [];
        }

        public bool IsEmpty(object value)
        {
            return string.IsNullOrWhiteSpace(value?.ToString());
        }
    }
}
