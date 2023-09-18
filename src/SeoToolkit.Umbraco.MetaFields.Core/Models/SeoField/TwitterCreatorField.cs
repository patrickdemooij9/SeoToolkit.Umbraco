using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(603)]
public class TwitterCreatorField : SeoField<string>
{
    public override string Title => "Twitter Creator";
    public override string Alias => SeoFieldAliasConstants.TwitterCreator;
    public override string Description => "Twitter Creator for the content";
    public override string GroupAlias => SeoFieldGroupConstants.OpenGraphGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

    public TwitterCreatorField()
    {
        var propertyEditor = new SeoFieldPropertyEditor("textbox");
        propertyEditor.SetExtraInformation("Provide the Twitter username of the content creator");

        Editor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        // Check for the "@" at the start of the value and remove if present - we'll add it in.
        if (value.StartsWith("@")) {
            value = value[1..];
        }
        return new HtmlString(value.IsNullOrWhiteSpace() ? null : $"<meta name=\"twitter:creator\" content=\"@{value}\"/>");
    }
}