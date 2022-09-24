using System;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.EventArgs;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteCrawler
    {
        event EventHandler<PageCrawlCompleteArgs> OnPageCrawlCompleted;

        Task Crawl(Uri startingUrl, int maxUrlsToCrawl, int delayBetweenRequests = 0);
        void StopCrawl();
    }
}
