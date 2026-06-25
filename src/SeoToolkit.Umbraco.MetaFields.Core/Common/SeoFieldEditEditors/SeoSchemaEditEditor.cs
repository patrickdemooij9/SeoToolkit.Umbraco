using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors
{
    public class SeoSchemaEditEditor : ISeoFieldEditEditor
    {
        public string View => "SeoToolkit.SchemaEditor";
        public Dictionary<string, object> Config { get; } = new();
        public IEditorValueConverter ValueConverter { get; }

        public SeoSchemaEditEditor()
        {
            ValueConverter = new SchemaEditorValueConverter();
        }
    }
}
