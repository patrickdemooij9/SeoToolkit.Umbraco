using System;
using Microsoft.AspNetCore.Html;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoField
    {
        string Title { get; }
        string Alias { get; }
        string Description { get; }
        Type FieldType { get; }
        ISeoFieldEditor Editor { get; }
        ISeoFieldEditEditor EditEditor { get; }
        //ISeoFieldRenderer Renderer { get; }

        HtmlString Render(object value);
    }
}
