using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Converters.EditorConverters
{
    public abstract class UmbracoUdiConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            return UdiParser.TryParse(value?.ToString(), out _) ? value : null;
        }

        public object ConvertObjectToEditorValue(object value)
        {
            if (value is IPublishedContent content)
            {
                return new GuidUdi(content.ItemType.ToString(), content.Key);
            }

            return null;
        }

        public abstract object ConvertDatabaseToObject(object value);

        public bool IsEmpty(object value)
        {
            var udi = value as Udi;
            return udi != null;
        }
    }
}
