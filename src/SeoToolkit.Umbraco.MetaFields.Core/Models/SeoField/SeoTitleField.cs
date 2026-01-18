using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;
using System.Web;
using SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(100)]
    public class SeoTitleField : ISeoField
    {
        private readonly ISeoKeyValueService _seoKeyValueService;

        public string Title => "Title";
        public string Alias => SeoFieldAliasConstants.Title;
        public string Description => "Title for the page";
        public string GroupAlias => SeoFieldGroupConstants.MetaFieldsGroup;
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea", "Umbraco.TinyMCE", "Umbraco.RichText" });
        public ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public SeoTitleField(ISeoKeyValueService seoKeyValueService)
        {
            _seoKeyValueService = seoKeyValueService;
        }

        public HtmlString Render(object value)
        {
            var template = _seoKeyValueService.GetValue("pageTitleTemplate").IfNullOrWhiteSpace("%value%");
            var templatedValue = template.Replace("%value%", value.ToString());

            return new HtmlString($"<title>{HttpUtility.HtmlEncode(templatedValue)}</title>");
        }
    }
}
