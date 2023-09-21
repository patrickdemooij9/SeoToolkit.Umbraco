using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;
using Umbraco.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(601)]
public class TwitterCardTypeField : SeoField<string>
{
    public override string Title => "Twitter Card Type";
    public override string Alias => SeoFieldAliasConstants.TwitterCardType;
    public override string Description => "Twitter Card Type for your page";
    public override string GroupAlias => SeoFieldGroupConstants.TwitterGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor { get; }

    public TwitterCardTypeField()
    {
        // App is relevant to direct download links to mobile apps; Player is relevant to video/audio media.
        var items = new string[] {
            "summary",
            "summary_large_image",
            "app",
            "player"
        };

        var propertyEditor = new SeoDropdownEditEditor(items);
        // propertyEditor.SetExtraInformation("Use one of 'summary', 'summary_large_image', 'app', 'player' or 'product'");
        // propertyEditor.SetDefaultValue("summary");

        Editor = new DropdownFieldPropertyEditor(items);

        EditEditor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        if (value is null) {
            return HtmlString.Empty;
        }
        return new HtmlString(value.IsNullOrWhiteSpace() ? null : $"<meta name=\"twitter:card\" content=\"{value}\"/>");
    }
}