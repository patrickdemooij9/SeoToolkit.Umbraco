using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    public interface ISitemapNoIndexFilter
    {
        bool IsNoIndex(IPublishedContent content, string culture);
    }
}
