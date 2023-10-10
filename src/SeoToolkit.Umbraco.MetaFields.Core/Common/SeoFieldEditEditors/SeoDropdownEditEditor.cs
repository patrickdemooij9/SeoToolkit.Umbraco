using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors
{
    public class SeoDropdownEditEditor : ISeoFieldEditEditor
    {
        public string View => "/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/PropertyEditor/dropdownList.html";
        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter { get; }

        public SeoDropdownEditEditor(string[] items)
        {
            ValueConverter = new TextValueConverter();
            Config = new Dictionary<string, object>
            {
                {"items",  items}
            };
        }
    }
}
