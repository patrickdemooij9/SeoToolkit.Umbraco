using System.Collections.Generic;
using uSeoToolkit.Umbraco.MetaFields.Core.Converters.EditorConverters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditEditors
{
    public class SeoTextBoxEditEditor : ISeoFieldEditEditor
    {
        public string View => "Textbox";
        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter { get; }

        public SeoTextBoxEditEditor()
        {
            ValueConverter = new TextValueConverter();
        }
    }
}
