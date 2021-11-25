using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Converters.SeoValueConverters
{
    public class FieldSeoValueConverter : ISeoValueConverter
    {
        public Type FromValue => typeof(FieldsModel);
        public Type ToValue => typeof(string);
        public object Convert(object value, IPublishedContent currentContent)
        {
            if (!(value is FieldsModel model))
                return null;

            foreach (var field in model.Fields)
            {
                var returnValue = currentContent.Value<string>(field);
                if (!string.IsNullOrWhiteSpace(returnValue))
                    return returnValue;
            }

            return null;
        }
    }
}
