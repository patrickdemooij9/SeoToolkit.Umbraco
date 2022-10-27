using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors
{
    public class SeoFieldPropertyEditor : ISeoFieldEditor, ISeoFieldEditorDefaultValue
    {
        private const string PreValueKey = "isPreValue";

        public string View => "/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/PropertyEditor/propertyEditor.html";

        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter { get; }

        public bool IsPreValue
        {
            get { return Config.ContainsKey(PreValueKey) && (bool)Config[PreValueKey]; }
            set { Config[PreValueKey] = value; }
        }

        private object _defaultValue;

        public SeoFieldPropertyEditor(string propertyView) : this(propertyView, new TextValueConverter())
        {
        }

        public SeoFieldPropertyEditor(string propertyView, IEditorValueConverter valueConverter)
        {
            ValueConverter = valueConverter;

            Config = new Dictionary<string, object>
            {
                {"view", propertyView}
            };
        }

        public void SetExtraInformation(string information)
        {
            Config["extraInformation"] = information;
        }

        public void SetDefaultValue(object value)
        {
            _defaultValue = value;
        }

        public object GetDefaultValue()
        {
            return _defaultValue;
        }
    }
}
