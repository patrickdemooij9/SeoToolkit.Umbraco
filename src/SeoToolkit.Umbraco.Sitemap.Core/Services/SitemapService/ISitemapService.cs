using System;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService
{
    public interface ISitemapService
    {
        void SetPageTypeSettings(SitemapPageSettings pageSettings);

        [Obsolete("Use GetPageTypeSettings(Guid contentTypeGuid) instead")]
        SitemapPageSettings GetPageTypeSettings(int contentTypeId);
        SitemapPageSettings? GetPageTypeSettings(Guid contentTypeGuid);
        SitemapPageSettings[] GetAll();
    }
}
