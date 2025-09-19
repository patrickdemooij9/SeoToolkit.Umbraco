using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System.Linq;
using System.Text.Json;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class CheckboxlistConverter : IEditorValueConverter
    {
        public object ConvertDatabaseToObject(object value)
        {
            if (value is null) return null;

            return value.ToString().Split(',');
        }

        public object ConvertEditorToDatabaseValue(object value)
        {
            var items = JsonHelpers.DeserializeArray<string>(value);
            if (items is null || items.Length == 0)
            {
                return null;
            }

            if (items.All(it => string.IsNullOrWhiteSpace(it.ToString()))) return null;

            return string.Join(',', items);
        }

        public object ConvertObjectToEditorValue(object value)
        {
            return value;
        }

        public bool IsEmpty(object value)
        {
            if ((value as string[])?.Length > 0) return false;
            return true;
        }
    }
}
