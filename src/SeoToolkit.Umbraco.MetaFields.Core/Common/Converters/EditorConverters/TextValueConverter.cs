using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class TextValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            return value?.ToString();
        }

        public object ConvertObjectToEditorValue(object value)
        {
            return value?.ToString();
        }

        public object ConvertDatabaseToObject(object value)
        {
            return value?.ToString();
        }

        public bool IsEmpty(object value)
        {
            return string.IsNullOrWhiteSpace(value?.ToString());
        }
    }
}
