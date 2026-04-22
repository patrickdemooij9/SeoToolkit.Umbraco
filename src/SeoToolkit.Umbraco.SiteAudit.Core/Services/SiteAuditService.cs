using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.EventArgs;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Notifications;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    public class SiteAuditService
    {
        private readonly ISiteAuditRepository _siteAuditRepository;
        private readonly ISiteCrawlerFactory _siteCrawlerFactory;
        private readonly ILogger<SiteAuditService> _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IScopeProvider _scopeProvider;
        private readonly IExternalSiteAuditClient _externalSiteAuditClient;

        private static readonly ConcurrentDictionary<ISiteCrawler, SiteAuditDto> CurrentlyRunningSiteAudits = new ConcurrentDictionary<ISiteCrawler, SiteAuditDto>();

        public SiteAuditService(ISiteAuditRepository siteAuditRepository,
            ISiteCrawlerFactory siteCrawlerFactory,
            ILogger<SiteAuditService> logger,
            IEventAggregator eventAggregator,
            IScopeProvider scopeProvider,
            IExternalSiteAuditClient externalSiteAuditClient)
        {
            _siteAuditRepository = siteAuditRepository;
            _siteCrawlerFactory = siteCrawlerFactory;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _scopeProvider = scopeProvider;
            _externalSiteAuditClient = externalSiteAuditClient;
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

            if (_externalSiteAuditClient.IsEnabled())
            {
                try
                {
                    var externalAuditId = await _externalSiteAuditClient.StartAudit(new ExternalSiteAuditCreateRequestDto
                    {
                        Name = model.Name,
                        StartingUrl = model.StartingUrl,
                        MaxPagesToCrawl = model.MaxPagesToCrawl,
                        DelayBetweenRequests = model.DelayBetweenRequests,
                        CheckIds = model.SiteChecks?.Select(it => it.Id).ToArray() ?? []
                    });
                    if (!externalAuditId.HasValue)
                    {
                        model.Status = SiteAuditStatus.Error;
                    }
                    else
                    {
                        model.ExternalAuditId = externalAuditId;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong while starting an external site audit!");
                    model.Status = SiteAuditStatus.Error;
                }

                Save(model);
                await _eventAggregator.PublishAsync(new SiteAuditUpdatedNotification(model));
                return model;
            }

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

        public async Task StopSiteAudit(int siteAuditId)
        {
            var persistentAudit = Get(siteAuditId);
            if (persistentAudit?.ExternalAuditId is not null)
            {
                await _externalSiteAuditClient.StopAudit(persistentAudit.ExternalAuditId.Value);
                return;
            }

            foreach (var (key, value) in CurrentlyRunningSiteAudits)
            {
                if (value.Id == siteAuditId)
                {
                    key.StopCrawl();
                    CurrentlyRunningSiteAudits.TryRemove(key, out _);
                }
            }
        }

        public SiteAuditDto Save(SiteAuditDto model)
        {
            if (!model.Persistent) return model;

            using var scope = _scopeProvider.CreateScope();
            model = model.Id == 0 ? _siteAuditRepository.Add(model) : _siteAuditRepository.Update(model);
            scope.Complete();
            return model;
        }

        public SiteAuditDto Get(int id)
        {
            var currentlyRunningAudit = CurrentlyRunningSiteAudits.Values.FirstOrDefault(it => it.Id == id);
            if (currentlyRunningAudit is null)
            {
                using var scope = _scopeProvider.CreateScope(autoComplete: true);
                return _siteAuditRepository.Get(id);
            }
            return currentlyRunningAudit;
        }

        public async Task<SiteAuditDetailViewModel> GetDetail(int id)
        {
            var model = Get(id);
            if (model is null)
            {
                return null;
            }

            var localDetail = new SiteAuditDetailViewModel(model);
            if (!model.ExternalAuditId.HasValue || !_externalSiteAuditClient.IsEnabled())
            {
                return localDetail;
            }

            try
            {
                var remoteDetail = await _externalSiteAuditClient.GetAuditDetail(model.ExternalAuditId.Value);
                if (remoteDetail is null)
                {
                    return localDetail;
                }

                remoteDetail.Id = model.Id;
                remoteDetail.Name ??= model.Name;
                remoteDetail.MaxPagesToCrawl ??= model.MaxPagesToCrawl;
                remoteDetail.Checks ??= localDetail.Checks;
                remoteDetail.Status ??= model.Status.ToString();

                var hasExternalStatus = Enum.TryParse<SiteAuditStatus>(remoteDetail.Status, true, out var externalStatus);
                var hasStatusChanges = hasExternalStatus && externalStatus != model.Status;
                if (remoteDetail.TotalPagesFound != model.TotalPagesFound || hasStatusChanges)
                {
                    model.TotalPagesFound = remoteDetail.TotalPagesFound;
                    if (hasExternalStatus)
                    {
                        model.Status = externalStatus;
                    }
                    Save(model);
                }

                return remoteDetail;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load external site audit details for audit {AuditId}", id);
                return localDetail;
            }
        }

        public IEnumerable<SiteAuditDto> GetAll()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _siteAuditRepository.GetAll().ToArray();
        }

        public void Delete(int id)
        {
            using var scope = _scopeProvider.CreateScope();
            _siteAuditRepository.Delete(id);
            scope.Complete();
        }

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
            crawledPage.Results.AddRange(auditModel.SiteChecks?.SelectMany(it => it.Check.RunCheck(args.Page, args.Context).Select(p => new PageCrawlResult(it, p))) ?? Enumerable.Empty<PageCrawlResult>());

            if (auditModel.Persistent)
            {
                using var scope = _scopeProvider.CreateScope();
                _siteAuditRepository.SaveCrawledPage(auditModel, crawledPage);
                scope.Complete();
            }

            auditModel.AddPage(crawledPage);

            //TODO: Should I save here?
            auditModel.TotalPagesFound = args.TotalPagesFound;

            _eventAggregator.Publish(new SiteAuditUpdatedNotification(auditModel));
        }
    }
}
