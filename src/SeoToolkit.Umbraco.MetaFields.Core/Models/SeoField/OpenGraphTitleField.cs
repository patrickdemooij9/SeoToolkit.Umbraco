using System;
using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(300)]
    public class OpenGraphTitleField : ISeoField
    {
        public string Title => "Open Graph Title";
        public string Alias => SeoFieldAliasConstants.OpenGraphTitle;
        public string Description => "Title for open graph";
        public string GroupAlias => SeoFieldGroupConstants.OpenGraphGroup;
        public Type FieldType => typeof(string);

        public  ISeoFieldEditor Editor { get; }
        public  ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

        public OpenGraphTitleField()
        {
            var propertyEditor = new SeoFieldPropertyEditor("textbox");
            propertyEditor.SetExtraInformation("You can use %CurrentPageName% to display the Title of the current item");
            propertyEditor.SetDefaultValue("%CurrentPageName%");

            Editor = propertyEditor;
        }
        
        public HtmlString Render(object value)
        {
            return new HtmlString($"<meta property=\"og:title\" content=\"{value}\"/>");
        }
    }
}
