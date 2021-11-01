using System;
using System.Threading.Tasks;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.EventArgs;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteCrawler
    {
        event EventHandler<PageCrawlCompleteArgs> OnPageCrawlCompleted;

        Task Crawl(Uri startingUrl, int maxUrlsToCrawl, int delayBetweenRequests = 0);
    }
}
