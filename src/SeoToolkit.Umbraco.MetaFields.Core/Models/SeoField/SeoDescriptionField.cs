using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(200)]
    public class SeoDescriptionField : ISeoField
    {
        public string Title => "Meta Description";
        public string Alias => SeoFieldAliasConstants.MetaDescription;
        public string Description => "Meta description for the page";
        public string GroupAlias => SeoFieldGroupConstants.MetaFieldsGroup;
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea", "Umbraco.TinyMCE" });
        public ISeoFieldEditEditor EditEditor => new SeoTextAreaEditEditor();

        public HtmlString Render(object value)
        {
            return new HtmlString($"<meta name=\"description\" content=\"{value}\"/>");
        }
    }
}
