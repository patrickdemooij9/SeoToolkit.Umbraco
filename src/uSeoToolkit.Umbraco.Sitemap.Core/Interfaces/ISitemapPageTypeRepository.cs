using uSeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    public interface ISitemapPageTypeRepository
    {
        void Set(SitemapPageSettings settings);
        SitemapPageSettings Get(int contentTypeId);
    }
}
