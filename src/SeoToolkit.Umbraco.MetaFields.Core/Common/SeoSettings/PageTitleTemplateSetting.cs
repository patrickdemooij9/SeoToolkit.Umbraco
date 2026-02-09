using SeoToolkit.Umbraco.Common.Core.Interfaces;
using System;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoSettings
{
    public class PageTitleTemplateSetting : ISeoKeyValueSetting
    {
        public string Key => "pageTitleTemplate";

        public string Title => "Page title template";

        public string Description => "The template that your page title will follow. %value% will be replaced with the SEO value";

        public string PropertyAlias => "Umb.PropertyEditorUi.TextBox";

        public Type EditorType => typeof(string);
    }
}
