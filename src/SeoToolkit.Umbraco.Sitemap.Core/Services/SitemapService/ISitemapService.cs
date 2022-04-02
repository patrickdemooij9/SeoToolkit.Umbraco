using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService
{
    public interface ISitemapService
    {
        void SetPageTypeSettings(SitemapPageSettings pageSettings);
        SitemapPageSettings GetPageTypeSettings(int contentTypeId);
    }
}
