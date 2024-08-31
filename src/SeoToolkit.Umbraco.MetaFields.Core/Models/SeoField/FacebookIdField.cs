using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(701)]
public class FacebookIdField : SeoField<string>
{
    public override string Title => "App Id";
    public override string Alias => SeoFieldAliasConstants.FacebookId;
    public override string Description => "Facebook app_id for the content";
    public override string GroupAlias => SeoFieldGroupConstants.SocialMediaGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

    public FacebookIdField()
    {
        var propertyEditor = new SeoFieldPropertyEditor("textbox");
        propertyEditor.SetExtraInformation("Provide the Facebook app_id associated with this content");

        Editor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        return new HtmlString(value.IsNullOrWhiteSpace() ? string.Empty : $"<meta property=\"fb:app_id\" content=\"{value}\"/>");
    }
}