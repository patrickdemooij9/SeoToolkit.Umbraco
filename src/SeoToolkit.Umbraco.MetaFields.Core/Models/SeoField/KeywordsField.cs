using System;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;

[Weight(200)]
public class KeywordsField : ISeoField
{
    public string Title => "Keywords";
    public string Alias => SeoFieldAliasConstants.Keywords;
    public string Description => "Keywords for the page";
    public string GroupAlias => SeoFieldGroupConstants.MetaFieldsGroup;
    public Type FieldType => typeof(string);

    public ISeoFieldEditor Editor => new KeywordsFieldPropertyEditor();
    public ISeoFieldEditEditor EditEditor => new SeoKeywordsEditor();

    public HtmlString Render(object value)
    {
        if (value is not string s) return null;
        return string.IsNullOrEmpty(s) ? null : new HtmlString($"<meta name=\"keywords\" content=\"{string.Join(",", JsonConvert.DeserializeObject<string[]>(s))}\"/>");
    }
}