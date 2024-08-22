using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(600)]
public class OpenGraphSiteNameField : SeoField<string>
{
    public override string Title => "Open Graph Site Name";
    public override string Alias => SeoFieldAliasConstants.OpenGraphSiteName;
    public override string Description => "Defines the website name.";
    public override string GroupAlias => SeoFieldGroupConstants.OpenGraphGroup;
    public override ISeoFieldEditor Editor { get; }
    public override ISeoFieldEditEditor EditEditor => new SeoTextBoxEditEditor();

    public OpenGraphSiteNameField()
    {
        var propertyEditor = new SeoFieldPropertyEditor("textbox");
        // propertyEditor.SetExtraInformation("Defines the content language.");
        // propertyEditor.SetDefaultValue("%CurrentLang%");
       
        Editor = propertyEditor;
    }

    protected override HtmlString Render(string value)
    {
        return new HtmlString(value.IsNullOrWhiteSpace() ? null : $"<meta name=\"og:site_name\" content=\"{value}\"/>");
    }
}