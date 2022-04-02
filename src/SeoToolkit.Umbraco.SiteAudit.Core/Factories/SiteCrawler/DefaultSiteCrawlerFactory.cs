using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.SiteCrawler;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler
{
    public class DefaultSiteCrawlerFactory : ISiteCrawlerFactory
    {
        public ISiteCrawler CreateNew()
        {
            return new Core.SiteCrawler.SiteCrawler(new DefaultPageUrlRequester(), new DefaultScheduler(), new DefaultLinkParser());
        }
    }
}
