using SeoToolkit.Umbraco.Common.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.NotFound.Core.Settings
{
    public class NotFoundPageSeoSetting : ISeoKeyValueSetting
    {
        public string Key => NotFoundConstants.NotFoundKeyValueKey;

        public string Title => "Page not found";

        public string Description => "Select your 404 page here.";

        public string PropertyAlias => "Umb.PropertyEditorUi.DocumentPicker";

        public Type EditorType => typeof(string);
    }
}
