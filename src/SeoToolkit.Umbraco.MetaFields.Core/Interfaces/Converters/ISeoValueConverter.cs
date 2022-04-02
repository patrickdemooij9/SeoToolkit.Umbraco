using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters
{
    public interface ISeoValueConverter
    {
        Type FromValue { get; }
        Type ToValue { get; }

        object Convert(object value, IPublishedContent currentContent, string fieldAlias);
    }
}
