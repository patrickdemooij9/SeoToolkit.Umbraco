using System;
using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(400)]
    public class OpenGraphDescriptionField : ISeoField
    {
        public string Title => "Open Graph Description";
        public string Alias => SeoFieldAliasConstants.OpenGraphDescription;
        public string Description => "Description for Open Graph";
        public string GroupAlias => SeoFieldGroupConstants.OpenGraphGroup;
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] {
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextArea,
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox,
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TinyMce,
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockGrid,
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockList,
            });
        public ISeoFieldEditEditor EditEditor => new SeoTextAreaEditEditor();

        public HtmlString Render(object value)
        {
            return new HtmlString($"<meta property=\"og:description\" content=\"{value}\"/>");
        }
    }
}
