using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using System;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService
{
    public class SitemapService : ISitemapService
    {
        private readonly ISitemapPageTypeRepository _sitemapPageTypeRepository;
        private readonly IEventAggregator _eventAggregator;

        public SitemapService(ISitemapPageTypeRepository sitemapPageTypeRepository, IEventAggregator eventAggregator)
        {
            _sitemapPageTypeRepository = sitemapPageTypeRepository;
            _eventAggregator = eventAggregator;
        }

        public void SetPageTypeSettings(SitemapPageSettings pageSettings)
        {
            _sitemapPageTypeRepository.Set(pageSettings);
            _eventAggregator.Publish(new SitemapPageSettingsSavedNotification(pageSettings));
        }

        public SitemapPageSettings GetPageTypeSettings(int contentTypeId)
        {
            return _sitemapPageTypeRepository.Get(contentTypeId);
        }

        public SitemapPageSettings GetPageTypeSettings(Guid contentTypeGuid)
        {
            return _sitemapPageTypeRepository.Get(contentTypeGuid);
        }

        public SitemapPageSettings[] GetAll()
        {
            return _sitemapPageTypeRepository.GetAll();
        }
    }
}
