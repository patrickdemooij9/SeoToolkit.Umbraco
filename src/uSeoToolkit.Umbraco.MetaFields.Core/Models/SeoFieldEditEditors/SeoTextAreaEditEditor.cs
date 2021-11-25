using System.Collections.Generic;
using uSeoToolkit.Umbraco.MetaFields.Core.Converters.EditorConverters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditEditors
{
    public class SeoTextAreaEditEditor : ISeoFieldEditEditor
    {
        public string View => "Textarea";
        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter { get; }

        public SeoTextAreaEditEditor()
        {
            ValueConverter = new TextValueConverter();
        }
    }
}
