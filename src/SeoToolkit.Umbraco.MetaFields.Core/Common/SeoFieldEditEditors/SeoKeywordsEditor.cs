using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors
{
    public class SeoKeywordsEditor : ISeoFieldEditEditor
    {
        public string View => "tags";
        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter { get; }

        public SeoKeywordsEditor()
        {
            Config = new Dictionary<string, object>
            {
                { "group", "keywords" },
                { "storageType", "Json" }
            };
            ValueConverter = new TextValueConverter();
        }
    }
}
