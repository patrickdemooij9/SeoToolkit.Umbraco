using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    public interface ISitemapCollectionProvider
    {
        SitemapNodeItem[] GetItems();
    }
}
