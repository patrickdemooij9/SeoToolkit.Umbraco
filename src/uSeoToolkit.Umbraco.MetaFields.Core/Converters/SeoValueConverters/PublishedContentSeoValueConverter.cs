using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Converters.SeoValueConverters
{
    public class PublishedContentSeoValueConverter : ISeoValueConverter
    {
        public Type FromValue => typeof(IPublishedContent);
        public Type ToValue => typeof(string);
        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            return (value as IPublishedContent)?.Url(mode: UrlMode.Absolute);
        }
    }
}
