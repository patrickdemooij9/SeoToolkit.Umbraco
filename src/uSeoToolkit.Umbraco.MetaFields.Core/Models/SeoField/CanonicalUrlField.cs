using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using uSeoToolkit.Umbraco.MetaFields.Core.Constants;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(600)]
    public class CanonicalUrlField : SeoField<string>
    {
        public override string Title => "Canonical Url";
        public override string Alias => SeoFieldAliasConstants.CanonicalUrl;
        public override string Description => "Canonical Url for the content";
        public override ISeoFieldEditor Editor { get; }
        public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public CanonicalUrlField()
        {
            var propertyEditor = new SeoFieldPropertyEditor("textbox");

            Editor = propertyEditor;
        }

        protected override HtmlString Render(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new HtmlString($"<link rel='canonical' href='{value}'/>");
        }
    }
}
