using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Sitemap.Core.Collections
{
    public class SitemapCollectionProviderCollectionBuilder : OrderedCollectionBuilderBase<SitemapCollectionProviderCollectionBuilder, SitemapCollectionProviderCollection, ISitemapCollectionProvider>
    {
        protected override SitemapCollectionProviderCollectionBuilder This => this;
    }
}
