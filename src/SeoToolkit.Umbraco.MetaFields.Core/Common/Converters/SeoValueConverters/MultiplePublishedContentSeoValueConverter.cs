using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class MultiplePublishedContentSeoValueConverter : ISeoValueConverter
    {
        public Type FromValue => typeof(IEnumerable<IPublishedContent>);
        public Type ToValue => typeof(string);
        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            if (value is not IEnumerable<IPublishedContent> castedValue)
                return null;

            return castedValue.FirstOrDefault()?.Url(mode: UrlMode.Absolute);
        }
    }
}
