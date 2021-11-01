using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.SiteCrawler;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler
{
    public class DefaultSiteCrawlerFactory : ISiteCrawlerFactory
    {
        public ISiteCrawler CreateNew()
        {
            return new Core.SiteCrawler.SiteCrawler(new DefaultPageUrlRequester(), new DefaultScheduler(), new DefaultLinkParser());
        }
    }
}
