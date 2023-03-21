using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(600)]
    public class CanonicalUrlField : SeoField<string>
    {
        public override string Title => "Canonical Url";
        public override string Alias => SeoFieldAliasConstants.CanonicalUrl;
        public override string Description => "Canonical Url for the content";
        public override string GroupAlias => SeoFieldGroupConstants.MetaFieldsGroup;
        public override ISeoFieldEditor Editor { get; }
        public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public CanonicalUrlField()
        {
            var propertyEditor = new SeoFieldPropertyEditor("textbox");
            propertyEditor.SetExtraInformation("You can use %CurrentUrl% to display the URL of the current item");
            propertyEditor.SetDefaultValue("%CurrentUrl%");

            Editor = propertyEditor;
        }

        protected override HtmlString Render(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new HtmlString($"<link rel=\"canonical\" href=\"{value}\"/>");
        }
    }
}
