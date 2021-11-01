using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler
{
    public interface ISiteCrawlerFactory
    {
        ISiteCrawler CreateNew();
    }
}
