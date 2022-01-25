using uSeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService
{
    public interface ISitemapService
    {
        void SetPageTypeSettings(SitemapPageSettings pageSettings);
        SitemapPageSettings GetPageTypeSettings(int contentTypeId);
    }
}
