using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
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
