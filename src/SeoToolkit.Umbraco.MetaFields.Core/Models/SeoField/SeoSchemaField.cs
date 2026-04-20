using Schema.NET;
using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using System;
using System.Linq;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(200)]
    public class SeoSchemaField : ISeoField
    {
        public string Title => "Schema";
        public string Alias => SeoFieldAliasConstants.Schema;
        public string Description => "The schemas are a set of 'types', each associated with a set of properties. The types are arranged in a hierarchy.";
        public string GroupAlias => SeoFieldGroupConstants.Others;
        public Type FieldType => typeof(IThing[]);

        public ISeoFieldEditor Editor => new SeoFieldPropertyEditor("SeoToolkit.SchemaEditor", new SchemaEditorValueConverter());
        public ISeoFieldEditEditor EditEditor => new SeoSchemaEditEditor();

        public HtmlString Render(object value)
        {
            if (value is not IThing[] schemas || schemas.Length == 0)
                return HtmlString.Empty;

            var schemaTags = schemas
                .Where(schema => schema is not null)
                .Select(schema => $"<script type=\"application/ld+json\">{schema}</script>");

            return new HtmlString(string.Join(Environment.NewLine, schemaTags));
        }
    }
}
