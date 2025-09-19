using System;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public interface IApiDataHandler
    {
        public string Name { get; }
        public object GetData(IPublishedContent content);
    }
}
