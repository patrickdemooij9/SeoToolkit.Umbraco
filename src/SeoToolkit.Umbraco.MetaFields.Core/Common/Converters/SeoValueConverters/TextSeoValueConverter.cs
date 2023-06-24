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
            var stringValue = value?.ToString();
            if (stringValue?.Contains("%CurrentUrl%") is true)
            {
                var url = currentContent.Url(mode: UrlMode.Absolute);
                if (url.Equals("#")) return ""; //No URL yet.
                if (new Uri(url).PathAndQuery.Equals("/"))
                {
                    url = url.TrimEnd('/');
                }
                return stringValue.Replace("%CurrentUrl%", url);
            }
            return stringValue;
        }
    }
}
