using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.EventArgs;

namespace SeoToolkit.Umbraco.SiteAudit.Core.SiteCrawler
{
    public class SiteCrawler : ISiteCrawler
    {
        private readonly IPageRequester _pageRequester;
        private readonly IScheduler _scheduler;
        private readonly ILinkParser _linkParser;

        private bool _crawlComplete = false;
        private SiteAuditContext _context;

        public event EventHandler<PageCrawlCompleteArgs> OnPageCrawlCompleted;

        public SiteCrawler(
            IPageRequester pageRequester,
            IScheduler scheduler,
            ILinkParser linkParser)
        {
            _pageRequester = pageRequester ?? new DefaultPageUrlRequester();
            _scheduler = scheduler ?? new DefaultScheduler();
            _linkParser = linkParser ?? new DefaultLinkParser();

            _context = new SiteAuditContext();
        }

        public async Task Crawl(Uri startingUrl, int maxUrlsToCrawl, int delayBetweenRequests = 0)
        {
            var maxCrawls = maxUrlsToCrawl;
            var currentCrawls = 0;

            _scheduler.Add(startingUrl);

            while (!_crawlComplete && currentCrawls < maxCrawls)
            {
                var linksLeftToSchedule = _scheduler.Count;
                if (linksLeftToSchedule > 0)
                {
                    var linkToCrawl = _scheduler.GetNext();
                    if (delayBetweenRequests > 0 && currentCrawls > 0)
                        Thread.Sleep(TimeSpan.FromMilliseconds(delayBetweenRequests));

                    //Check if a check has already visited it for us and returned an invalid code
                    var cachedStatusCode = _context.GetStatusCode(linkToCrawl);
                    if (cachedStatusCode != null && (cachedStatusCode < 200 || cachedStatusCode > 299))
                    {
                        ProcessPage(new CrawledPageModel(linkToCrawl) { StatusCode = cachedStatusCode.Value });
                    }
                    else
                    {
                        ProcessPage(await _pageRequester.MakeRequest(linkToCrawl));
                    }
                    
                    currentCrawls++;
                }
                else
                {
                    //Finished the crawl
                    _crawlComplete = true;
                }
            }
        }

        private void ProcessPage(CrawledPageModel page)
        {
            //We keep the urls in a context, so that any checks could benefit from it
            _context.AddUrlIfNotPresent(page.Url, page.StatusCode);

            var links = _linkParser.GetLinks(page).ToArray();
            page.FoundUrls = links;
            foreach (var link in links)
            {
                if (!_scheduler.IsUriKnown(link) && page.Url.Authority == link.Authority)
                {
                    _scheduler.Add(link);
                }
            }
            _scheduler.AddKnownUri(page.Url);
            OnPageCrawlCompleted?.Invoke(this, new PageCrawlCompleteArgs() { Page = page, Context = _context, TotalPagesFound = _scheduler.TotalCount});
        }

        public void StopCrawl()
        {
            _crawlComplete = true;
        }
    }
}
