using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(602)]
public class TwitterSiteField : SeoField<string>
{
    public override string Title => "Twitter Site";
    public override string Alias => SeoFieldAliasConstants.TwitterSite;
    public override string Description => "Twitter Site for the content";
    public override string GroupAlias => SeoFieldGroupConstants.OpenGraphGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

    public TwitterSiteField()
    {
        var propertyEditor = new SeoFieldPropertyEditor("textbox");
        propertyEditor.SetExtraInformation("Provide the Twitter username of the website");

        Editor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        // Check for the "@" at the start of the value and remove if present - we'll add it in.
        if (value.StartsWith("@")) {
            value = value[1..];
        }
        return new HtmlString(value.IsNullOrWhiteSpace() ? string.Empty : $"<meta name=\"twitter:site\" content=\"@{value}\"/>");
    }
}