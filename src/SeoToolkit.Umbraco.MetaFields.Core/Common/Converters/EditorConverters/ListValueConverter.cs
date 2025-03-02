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
            return JsonConvert.DeserializeObject<string[]>(value?.ToString() ?? "[]");
        }

        public bool IsEmpty(object value)
        {
            return string.IsNullOrWhiteSpace(value?.ToString());
        }
    }
}
