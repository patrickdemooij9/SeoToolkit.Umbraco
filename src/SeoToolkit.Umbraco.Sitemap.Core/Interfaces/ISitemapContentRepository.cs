using System;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    public interface ISitemapContentRepository
    {
        SitemapContentSettings? Get(Guid nodeKey);
        void Set(SitemapContentSettings settings);
        void Delete(Guid nodeKey);
        SitemapContentSettings[] GetAll();
    }
}
