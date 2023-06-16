using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(200)]
    public class SeoSchemaField : ISeoField
    {
        public string Title => "Schema";
        public string Alias => SeoFieldAliasConstants.Schema;
        public string Description => "The schemas are a set of 'types', each associated with a set of properties. The types are arranged in a hierarchy.";
        public string GroupAlias => SeoFieldGroupConstants.MetaFieldsGroup;
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.TextBox", "Umbraco.TextArea", "Umbraco.TinyMCE" });
        public ISeoFieldEditEditor EditEditor => new SeoTextAreaEditEditor();

        public HtmlString Render(object value)
        {
            return new HtmlString($"{value}");
        }
    }
}
