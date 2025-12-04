using System;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    public interface ISitemapPageTypeRepository
    {
        void Set(SitemapPageSettings settings);
        [Obsolete("Use Get(Guid contentTypeGuid) instead")]
        SitemapPageSettings Get(int contentTypeId);
        SitemapPageSettings Get(Guid contentTypeGuid);
        SitemapPageSettings[] GetAll();
    }
}
