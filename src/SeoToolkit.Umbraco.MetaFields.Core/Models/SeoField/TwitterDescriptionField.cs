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
    public class TwitterDescriptionField : ISeoField
    {
        public string Title => "Twitter Description";
        public string Alias => SeoFieldAliasConstants.TwitterDescription;
        public string Description => "Description for Twitter";
        public string GroupAlias => SeoFieldGroupConstants.TwitterGroup;
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
            return new HtmlString($"<meta name=\"twitter:description\" content=\"{value}\"/>");
        }
    }
}
