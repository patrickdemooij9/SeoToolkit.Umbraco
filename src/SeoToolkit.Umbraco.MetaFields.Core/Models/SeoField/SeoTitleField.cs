using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;
using System.Web;
using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldSuggestions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(100)]
    public class SeoTitleField : ISeoField, ISeoFieldHasSuggestions
    {
        public string Title => "Title";
        public string Alias => SeoFieldAliasConstants.Title;
        public string Description => "Title for the page";
        public string GroupAlias => SeoFieldGroupConstants.MetaFieldsGroup;
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea", "Umbraco.TinyMCE", "Umbraco.RichText" });
        public ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public List<ISeoFieldSuggestion> Suggestions { get; } = new List<ISeoFieldSuggestion>
        {
            new SeoFieldMaxLengthSuggestion() { MaxLength = 60 }
        };

        public HtmlString Render(object value)
        {
            return new HtmlString($"<title>{HttpUtility.HtmlEncode(value)}</title>");
        }
    }
}
