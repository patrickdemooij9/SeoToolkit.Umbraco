using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class SingleDropdownValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            var items = JsonHelpers.DeserializeArray<string>(value);
            return items?.FirstOrDefault() ?? value?.ToString();
        }

        public object ConvertObjectToEditorValue(object value)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return Array.Empty<string>();
            }
            return new string[] { value?.ToString() };
        }

        public object ConvertDatabaseToObject(object value)
        {
            return value;
        }

        public bool IsEmpty(object value)
        {
            return string.IsNullOrWhiteSpace(value?.ToString());
        }
    }
}
