using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using System;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Core.Connectors
{
    public class MetaFieldsNoIndexFilter : ISitemapNoIndexFilter
    {
        private readonly IMetaFieldsService _metaFieldsService;

        public MetaFieldsNoIndexFilter(IMetaFieldsService metaFieldsService)
        {
            _metaFieldsService = metaFieldsService;
        }

        public bool IsNoIndex(IPublishedContent content, string culture)
        {
            var metaTags = _metaFieldsService.Get(content, true);
            return metaTags?.Robots != null && Array.Exists(metaTags.Robots, r => r.Equals("noindex", StringComparison.OrdinalIgnoreCase));
        }
    }
}
