using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class SchemaProperty
    {
        public string Alias { get; set; }
        public string DisplayName { get; set; }
        public string PropertyEditor { get; set; }
        public bool AllowReference { get; set; }
        public IEditorValueConverter ValueConverter { get; set; }
        public Dictionary<string, object> Config { get; set; }

        public SchemaProperty(string alias, string displayName, string propertyEditor, bool allowReference = true, IEditorValueConverter valueConverter = null, Dictionary<string, object> config = null)
        {
            Alias = alias;
            DisplayName = displayName;
            PropertyEditor = propertyEditor;
            AllowReference = allowReference;
            ValueConverter = valueConverter ?? new TextValueConverter();
            Config = config ?? new Dictionary<string, object>();
        }
    }
}
