using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(600)]
public class TwitterUrlField : SeoField<string>
{
    public override string Title => "Twitter Url";
    public override string Alias => SeoFieldAliasConstants.TwitterUrl;
    public override string Description => "Twitter Url for the content";
    public override string GroupAlias => SeoFieldGroupConstants.TwitterGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

    public TwitterUrlField()
    {
        var propertyEditor = new SeoFieldPropertyEditor("textbox");
        propertyEditor.SetExtraInformation("You can use %CurrentUrl% to display the URL of the current item");
        propertyEditor.SetDefaultValue("%CurrentUrl%");

        Editor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        return new HtmlString(value.IsNullOrWhiteSpace() ? null : $"<meta name=\"twitter:url\" content=\"{value}\"/>");
    }
}