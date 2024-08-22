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
public class OpenGraphTypeField : SeoField<string>
{
    public override string Title => "Open Graph Type";
    public override string Alias => SeoFieldAliasConstants.OpenGraphType;
    public override string Description => "Open Graph Type for your page";
    public override string GroupAlias => SeoFieldGroupConstants.OpenGraphGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor { get; }

    public OpenGraphTypeField()
    {
        // App is relevant to direct download links to mobile apps; Player is relevant to video/audio media.
        var items = new string[] {
            "music.song",
            "music.album",
            "music.playlist",
            "music.radio_station",
            "video.movie",
            "video.episode",
            "video.tv_show",
            "video.other",
            "article",
            "book",
            "profile",
            "website"
           
        };

        var propertyEditor = new SeoDropdownEditEditor(items);
        // propertyEditor.SetExtraInformation("Use one of 'summary', 'summary_large_image', 'app', 'player' or 'product'");
        // propertyEditor.SetDefaultValue("summary");

        Editor = new DropdownFieldPropertyEditor(items,"article");

        EditEditor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        if (value is null) {
            return HtmlString.Empty;
        }
        return new HtmlString(value.IsNullOrWhiteSpace() ? null : $"<meta name=\"og:type\" content=\"{value}\"/>");
    }
}