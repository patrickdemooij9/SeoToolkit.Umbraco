using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
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
