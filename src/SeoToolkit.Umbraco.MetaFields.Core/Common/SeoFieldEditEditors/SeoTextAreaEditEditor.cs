using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors
{
    public class SeoTextAreaEditEditor : ISeoFieldEditEditor
    {
        public string View => "textarea";
        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter { get; }

        public SeoTextAreaEditEditor()
        {
            ValueConverter = new TextValueConverter();
        }
    }
}
