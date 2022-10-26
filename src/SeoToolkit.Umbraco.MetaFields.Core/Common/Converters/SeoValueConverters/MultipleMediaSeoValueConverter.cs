using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class MultipleMediaSeoValueConverter : ISeoValueConverter
    {
        public static List<string> SupportedMediaTypes = new List<string>
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".webp",
            ".gif"
        };

        public Type FromValue => typeof(IEnumerable<MediaWithCrops>);

        public Type ToValue => typeof(string);

        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            if (value is not IEnumerable<MediaWithCrops> castedValue)
                return null;

            return castedValue.FirstOrDefault(it => SupportedMediaTypes.Contains(Path.GetExtension(it.Url())))?.Url(mode: UrlMode.Absolute);
        }
    }
}
