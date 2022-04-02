using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Enums;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class FieldSeoValueConverter : ISeoValueConverter
    {
        private readonly SeoConverterCollection _converterCollection;
        private readonly FieldProviderCollection _fieldProviderCollection;

        public FieldSeoValueConverter(SeoConverterCollection converterCollection, FieldProviderCollection fieldProviderCollection)
        {
            _converterCollection = converterCollection;
            _fieldProviderCollection = fieldProviderCollection;
        }

        public Type FromValue => typeof(FieldsModel);
        public Type ToValue => typeof(string);
        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            if (!(value is FieldsModel model))
                return null;

            foreach (var field in model.Fields)
            {
                object returnValue;
                if (field.Source == FieldSourceType.PropertyFieldSource)
                    returnValue = currentContent.Value(field.Value);
                else
                    returnValue = _fieldProviderCollection.HandleItem(field, currentContent, fieldAlias);
                if (returnValue is null)
                    continue;

                var convertedValue = _converterCollection.GetConverter(returnValue.GetType(), typeof(string))
                    .Convert(returnValue, currentContent, fieldAlias) as string;
                if (!string.IsNullOrWhiteSpace(convertedValue))
                    return convertedValue;
            }

            return null;
        }
    }
}
