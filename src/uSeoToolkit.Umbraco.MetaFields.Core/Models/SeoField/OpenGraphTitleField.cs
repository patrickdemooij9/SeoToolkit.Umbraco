using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.MetaFields.Core.Constants;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditEditors;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(300)]
    public class OpenGraphTitleField : ISeoField
    {
        public string Title => "Open Graph Title";
        public string Alias => SeoFieldAliasConstants.OpenGraphTitle;
        public string Description => "Title for open graph";
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea" });
        public ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public HtmlString Render(object value)
        {
            return new HtmlString($"<meta property='og:title' content='{value}'/>");
        }
    }
}
