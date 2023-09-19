using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public abstract class BaseGridSeoValueConverter<T> : ISeoValueConverter
        where T : class, IBlockReference<IPublishedElement, IPublishedElement>
    {
        public virtual Type FromValue {get;}
        public virtual Type ToValue {get;}
        public virtual object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            var dataTypes = new string[]{
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextArea,
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox,
                global::Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TinyMce
            };
            // Walk the Block List and get any text values in a list.
            List<object> values = new();
            if (value is BlockModelCollection<T> blocks) {
                foreach(var item in blocks.Select(b => b.Content))
                {
                    // Go through each of the properties in the content element that are Text, Text Area or Rich Text and aggregate their values into a single string.
                    foreach(var property in item.Properties.Where(p => dataTypes.Contains(p.PropertyType.DataType.EditorAlias))) {
                        values.Add(item.Value(property.Alias));

                        // TODO: If there is an embedded Block Grid or Block List item, recurse into it and get the values.
                    }
                }
            }
            return values.Aggregate("", (a, b) => $"{a} {b}").Trim();
        }
    }
}
