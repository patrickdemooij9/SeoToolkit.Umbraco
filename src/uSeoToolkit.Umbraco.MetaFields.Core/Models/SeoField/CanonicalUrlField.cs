using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Constants;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditEditors;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(600)]
    public class CanonicalUrlField : ISeoField
    {
        public string Title => "Canonical Url";
        public string Alias => SeoFieldAliasConstants.CanonicalUrl;
        public string Description => "Canonical Url for the content";
        public Type FieldType => typeof(string);
        public ISeoFieldEditor Editor { get; }
        public ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public CanonicalUrlField()
        {
            var propertyEditor = new SeoFieldPropertyEditor("textbox", GetEditorValueTransformation);

            Editor = propertyEditor;
        }

        private static string GetEditorValueTransformation(IPublishedContent content, object value)
        {
            var valueString = value?.ToString();
            if (string.IsNullOrWhiteSpace(valueString))
                return string.Empty;

            return valueString.Replace("%CurrentUrl%", content.Url(mode: UrlMode.Absolute));
        }

        public HtmlString Render(object value)
        {
            return new HtmlString($"<link rel='canonical' href='{value}'/>");
        }
    }
}
