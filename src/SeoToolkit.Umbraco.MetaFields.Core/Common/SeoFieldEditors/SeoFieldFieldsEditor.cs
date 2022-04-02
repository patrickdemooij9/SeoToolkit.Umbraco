using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors
{
    public class SeoFieldFieldsEditor : ISeoFieldEditor
    {
        private readonly string[] _fieldTypes;
        public string View => "/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/FieldsEditor/fieldsEditor.html";
        public Dictionary<string, object> Config => new Dictionary<string, object>
        {
            {"dataTypes", _fieldTypes}
        };

        public IEditorValueConverter ValueConverter { get; }

        public SeoFieldFieldsEditor(string[] fieldTypes)
        {
            _fieldTypes = fieldTypes;

            ValueConverter = new FieldValueConverter();
        }
    }
}
