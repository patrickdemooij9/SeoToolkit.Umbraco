using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class TextSeoValueConverter : ISeoValueConverter
    {
        public Type FromValue => typeof(string);
        public Type ToValue => typeof(string);
        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            return value?.ToString()?.Replace("%CurrentUrl%", currentContent.Url(mode: UrlMode.Absolute));
        }
    }
}
