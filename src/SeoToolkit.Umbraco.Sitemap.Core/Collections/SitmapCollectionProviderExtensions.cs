using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Sitemap.Core.Collections
{
    public static class SitmapCollectionProviderExtensions
    {
        public static SitemapCollectionProviderCollectionBuilder SitemapCollections(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<SitemapCollectionProviderCollectionBuilder>();
    }
}
