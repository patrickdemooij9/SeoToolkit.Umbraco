using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    public interface ISitemapPageTypeRepository
    {
        void Set(SitemapPageSettings settings);
        SitemapPageSettings Get(int contentTypeId);
    }
}
