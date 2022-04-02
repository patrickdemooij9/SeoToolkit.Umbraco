using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService
{
    public class SitemapService : ISitemapService
    {
        private readonly ISitemapPageTypeRepository _sitemapPageTypeRepository;

        public SitemapService(ISitemapPageTypeRepository sitemapPageTypeRepository)
        {
            _sitemapPageTypeRepository = sitemapPageTypeRepository;
        }

        public void SetPageTypeSettings(SitemapPageSettings pageSettings)
        {
            _sitemapPageTypeRepository.Set(pageSettings);
        }

        public SitemapPageSettings GetPageTypeSettings(int contentTypeId)
        {
            return _sitemapPageTypeRepository.Get(contentTypeId);
        }
    }
}
