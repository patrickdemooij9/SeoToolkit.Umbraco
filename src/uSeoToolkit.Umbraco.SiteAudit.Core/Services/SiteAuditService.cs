using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;
using uSeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.EventArgs;
using uSeoToolkit.Umbraco.SiteAudit.Core.Notifications;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Services
{
    public class SiteAuditService
    {
        private readonly ISiteAuditRepository _siteAuditRepository;
        private readonly ISiteCrawlerFactory _siteCrawlerFactory;
        private readonly ILogger<SiteAuditService> _logger;
        private readonly IEventAggregator _eventAggregator;

        private static readonly ConcurrentDictionary<ISiteCrawler, SiteAuditDto> CurrentlyRunningSiteAudits = new ConcurrentDictionary<ISiteCrawler, SiteAuditDto>();

        public SiteAuditService(ISiteAuditRepository siteAuditRepository,
            ISiteCrawlerFactory siteCrawlerFactory,
            ILogger<SiteAuditService> logger,
            IEventAggregator eventAggregator)
        {
            _siteAuditRepository = siteAuditRepository;
            _siteCrawlerFactory = siteCrawlerFactory;
            _logger = logger;
            _eventAggregator = eventAggregator;
        }

        public async Task<SiteAuditDto> StartSiteAudit(SiteAuditDto model)
        {
            if (CurrentlyRunningSiteAudits.Values.Contains(model))
            {
                _logger.LogWarning("Site audit cannot be started as it is already running!");
                return model;
            }

            model.Status = SiteAuditStatus.Running;
            Save(model);

            var siteCrawler = _siteCrawlerFactory.CreateNew();
            CurrentlyRunningSiteAudits.TryAdd(siteCrawler, model);
            siteCrawler.OnPageCrawlCompleted += HandleChecks;
            try
            {
                await siteCrawler.Crawl(model.StartingUrl, model.MaxPagesToCrawl ?? int.MaxValue, model.DelayBetweenRequests);
                model.Status = SiteAuditStatus.Finished;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong!");
                model.Status = SiteAuditStatus.Error;
            }
            siteCrawler.OnPageCrawlCompleted -= HandleChecks;
            CurrentlyRunningSiteAudits.TryRemove(siteCrawler, out _);
                
            Save(model);
            await _eventAggregator.PublishAsync(new SiteAuditUpdatedNotification(model));

            return model;
        }

        public SiteAuditDto Save(SiteAuditDto model)
        {
            return model.Id == 0 ? _siteAuditRepository.Add(model) : _siteAuditRepository.Update(model);
        }

        public SiteAuditDto Get(int id) => _siteAuditRepository.Get(id);
        public IEnumerable<SiteAuditDto> GetAll() => _siteAuditRepository.GetAll();
        public void Delete(int id) => _siteAuditRepository.Delete(id);

        private void HandleChecks(object sender, PageCrawlCompleteArgs args)
        {
            var crawler = sender as ISiteCrawler;
            if (!CurrentlyRunningSiteAudits.TryGetValue(crawler, out var auditModel))
            {
                _logger.LogError("Could not find audit model for this crawler!");
                return;
            }
            var crawledPage = new CrawledPageDto
            {
                PageUrl = args.Page.Url,
                StatusCode = args.Page.StatusCode
            };
            crawledPage.Results.AddRange(auditModel.SiteChecks?.SelectMany(it => it.Check.RunCheck(args.Page).Select(p => new PageCrawlResult(it, p))) ?? Enumerable.Empty<PageCrawlResult>());

            _siteAuditRepository.SaveCrawledPage(auditModel, crawledPage);
            auditModel.AddPage(crawledPage);

            //TODO: Should I save here?
            auditModel.TotalPagesFound = args.TotalPagesFound;

            _eventAggregator.Publish(new SiteAuditUpdatedNotification(auditModel));
        }
    }
}
