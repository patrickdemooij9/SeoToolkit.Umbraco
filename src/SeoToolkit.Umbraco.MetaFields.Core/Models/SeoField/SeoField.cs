using System;
using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    public abstract class SeoField<T> : ISeoField
    {
        public abstract string Title { get; }
        public abstract string Alias { get; }
        public abstract string Description { get; }
        public abstract string GroupAlias { get; }
        public Type FieldType => typeof(T);
        public abstract ISeoFieldEditor Editor { get; }
        public abstract ISeoFieldEditEditor EditEditor { get; }
        public HtmlString Render(object value)
        {
            if (value is T castValue)
            {
                return Render(castValue);
            }

            return default;
        }

        protected abstract HtmlString Render(T value);
    }
}
