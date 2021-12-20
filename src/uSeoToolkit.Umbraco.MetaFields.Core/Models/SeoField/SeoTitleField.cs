using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using uSeoToolkit.Umbraco.MetaFields.Core.Constants;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(100)]
    public class SeoTitleField : ISeoField
    {
        public string Title => "Title";
        public string Alias => SeoFieldAliasConstants.Title;
        public string Description => "Title for the page";
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea" });
        public ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public HtmlString Render(object value)
        {
            return new HtmlString($"<title>{value}</title>");
        }
    }
}
