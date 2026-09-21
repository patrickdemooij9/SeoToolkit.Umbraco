using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoField
    {
        string Title { get; }
        string Alias { get; }
        string Description { get; }
        string GroupAlias { get; }
        Type FieldType { get; }
        ISeoFieldEditor Editor { get; }
        ISeoFieldEditEditor EditEditor { get; }
        //ISeoFieldRenderer Renderer { get; }

        /// <summary>
        /// When <c>true</c> (the default), an empty content value falls back to the value
        /// configured on the document type. Set to <c>false</c> for fields that should never
        /// inherit the document type value (e.g. the schema field, which merges document type
        /// schemas additively instead of falling back to them).
        /// </summary>
        bool AllowDocumentTypeFallback => true;

        HtmlString Render(object value);
    }
}
