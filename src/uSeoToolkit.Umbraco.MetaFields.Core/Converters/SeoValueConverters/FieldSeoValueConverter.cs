using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Converters.SeoValueConverters
{
    public class FieldSeoValueConverter : ISeoValueConverter
    {
        private readonly SeoConverterCollection _converterCollection;

        public FieldSeoValueConverter(SeoConverterCollection converterCollection)
        {
            _converterCollection = converterCollection;
        }

        public Type FromValue => typeof(FieldsModel);
        public Type ToValue => typeof(string);
        public object Convert(object value, IPublishedContent currentContent)
        {
            if (!(value is FieldsModel model))
                return null;

            foreach (var field in model.Fields)
            {
                var returnValue = currentContent.Value(field);
                if (returnValue is null)
                    continue;

                var convertedValue = _converterCollection.GetConverter(returnValue.GetType(), typeof(string))
                    .Convert(returnValue, currentContent) as string;
                if (!string.IsNullOrWhiteSpace(convertedValue))
                    return convertedValue;
            }

            return null;
        }
    }
}
