using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Converters.EditorConverters
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
