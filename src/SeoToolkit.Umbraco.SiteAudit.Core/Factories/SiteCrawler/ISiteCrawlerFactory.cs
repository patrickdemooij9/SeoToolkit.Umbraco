using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler
{
    public interface ISiteCrawlerFactory
    {
        ISiteCrawler CreateNew();
    }
}
