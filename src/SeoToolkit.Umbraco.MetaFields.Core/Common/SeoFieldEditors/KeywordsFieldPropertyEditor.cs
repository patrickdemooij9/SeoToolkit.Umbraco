using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;
using System.Linq;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors
{
    public class KeywordsFieldPropertyEditor : SeoFieldPropertyEditor, ISeoFieldEditorProcessor
    {

        public KeywordsFieldPropertyEditor() : base("Umb.PropertyEditorUi.Tags", new ListValueConverter())
        {
            IsPreValue = false;
            Config.Add("group", "keywords");
            Config.Add("storageType", "Json");

        }

        public object HandleValue(object value)
        {
            return value;
        }
    }
}
