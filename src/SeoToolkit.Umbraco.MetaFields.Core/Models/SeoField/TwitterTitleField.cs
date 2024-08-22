using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(602)]
public class TwitterTitleField : SeoField<string>
{
    public override string Title => "Twitter Title";
    public override string Alias => SeoFieldAliasConstants.TwitterTitle;
    public override string Description => "Twitter Name for the content";
    public override string GroupAlias => SeoFieldGroupConstants.TwitterGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

    public TwitterTitleField()
    {
        var propertyEditor = new SeoFieldPropertyEditor("textbox");
        propertyEditor.SetExtraInformation("Provide the Title of Page to show on Twitter");
        propertyEditor.SetDefaultValue("%CurrentPageName%");

        Editor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        return new HtmlString(value.IsNullOrWhiteSpace() ? string.Empty : $"<meta name=\"twitter:title\" content=\"{value}\"/>");
    }
}