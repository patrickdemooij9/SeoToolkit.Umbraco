using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class HtmlEncodedStringSeoConverter : ISeoValueConverter
    {
        public Type FromValue => typeof(IHtmlEncodedString);

        public Type ToValue => typeof(string);

        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            if (value is not IHtmlEncodedString encodedString) return null;

            return encodedString.ToHtmlString()?.StripHtml();
        }
    }
}
