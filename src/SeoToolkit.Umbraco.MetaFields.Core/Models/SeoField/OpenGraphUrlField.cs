using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(600)]
public class OpenGraphUrlField : SeoField<string>
{
    public override string Title => "Open Graph Url";
    public override string Alias => SeoFieldAliasConstants.OpenGraphUrl;
    public override string Description => "Open Graph Url for the content";
    public override string GroupAlias => SeoFieldGroupConstants.SocialMediaGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

    public OpenGraphUrlField()
    {
        var propertyEditor = new SeoFieldPropertyEditor("textbox");
        propertyEditor.SetExtraInformation("You can use %CurrentUrl% to display the URL of the current item");
        propertyEditor.SetDefaultValue("%CurrentUrl%");

        Editor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        return new HtmlString(value.IsNullOrWhiteSpace() ? null : $"<meta property=\"og:url\" content=\"{value}\"/>");
    }
}