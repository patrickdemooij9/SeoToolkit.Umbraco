using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;
using System.Linq;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors
{
    public class CheckboxListFieldPropertyEditor : SeoFieldPropertyEditor, ISeoFieldEditorProcessor
    {
        private readonly CheckboxItem[] _items;

        public CheckboxListFieldPropertyEditor(CheckboxItem[] items) : base("checkboxlist", new CheckboxlistConverter())
        {
            IsPreValue = true;
            Config.Add("prevalues", items);

            _items = items;
        }

        public object HandleValue(object value)
        {
            if (!(value is JArray array)) { return value; }

            var validValues = _items.Select(it => it.Value).ToArray();
            var invalidValues = array.Where(it => !validValues.Contains(it.ToString())).ToArray();

            foreach(var invalidValue in invalidValues)
            {
                array.Remove(invalidValue);
            }

            return array;
        }
    }
}
