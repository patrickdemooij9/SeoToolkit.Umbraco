using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors
{
    public class SeoFieldPropertyEditor : ISeoFieldEditor
    {
        private readonly string _propertyView;
        public string View => "/App_Plugins/uSeoToolkitMetaFields/Interface/SeoFieldEditors/PropertyEditor/propertyEditor.html";

        public Dictionary<string, object> Config => new Dictionary<string, object>
        {
            {"view", _propertyView}
        };

        public IEditorValueConverter ValueConverter { get; }

        public SeoFieldPropertyEditor(string propertyView, IEditorValueConverter valueConverter = null)
        {
            _propertyView = propertyView;

            ValueConverter = valueConverter ?? new TextValueConverter();
        }
    }
}
