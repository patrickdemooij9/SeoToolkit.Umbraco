using System;

namespace SeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    public interface ISitemapNoIndexFilter
    {
        void Prepare(string culture);
        bool IsNoIndex(Guid contentKey);
    }
}
