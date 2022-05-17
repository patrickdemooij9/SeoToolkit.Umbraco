using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(300)]
    public class OpenGraphTitleField : ISeoField
    {
        public string Title => "Open Graph Title";
        public string Alias => SeoFieldAliasConstants.OpenGraphTitle;
        public string Description => "Title for open graph";
        public string GroupAlias => SeoFieldGroupConstants.MetaFieldsGroup;
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea" });
        public ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public HtmlString Render(object value)
        {
            return new HtmlString($"<meta property='og:title' content='{value}'/>");
        }
    }
}
