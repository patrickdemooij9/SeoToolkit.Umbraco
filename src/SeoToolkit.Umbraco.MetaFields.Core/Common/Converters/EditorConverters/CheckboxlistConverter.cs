using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

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
            if (!(value is JArray array) || array.Count == 0) return null;
            return string.Join(',', array);
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
