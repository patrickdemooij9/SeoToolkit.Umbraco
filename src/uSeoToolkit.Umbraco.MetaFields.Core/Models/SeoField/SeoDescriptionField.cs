using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.MetaFields.Core.Constants;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditEditors;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(200)]
    public class SeoDescriptionField : ISeoField
    {
        public string Title => "Meta Description";
        public string Alias => SeoFieldAliasConstants.MetaDescription;
        public string Description => "Meta description for the page";
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea" });
        public ISeoFieldEditEditor EditEditor => new SeoTextAreaEditEditor();

        public HtmlString Render(object value)
        {
            return new HtmlString($"<meta name='description' content='{value}'/>");
        }
    }
}
