using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;
using System.Collections.Generic;
using System.Linq;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors
{
    public class SeoCheckboxlistEditEditor : ISeoFieldEditEditor
    {
        public string View => "checkboxlist";
        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter => new CheckboxlistConverter();

        public SeoCheckboxlistEditEditor(CheckboxItem[] items)
        {
            Config = new Dictionary<string, object>
            {
                {"prevalues",  items}
            };
        }
    }
}
