using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors
{
    public class DropdownFieldPropertyEditor : SeoFieldPropertyEditor, ISeoFieldEditorProcessor
    {

        public DropdownFieldPropertyEditor(string[] items,string defaultValue = "") : base("/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/PropertyEditor/dropdownList.html", new TextValueConverter())
        {
            IsPreValue = true;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                SetDefaultValue(defaultValue);
            }
          
            Config.Add("items", items);
        }

        public object HandleValue(object value)
        {
            return value;
        }
    }
}
