using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using System;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService
{
    public class SitemapService : ISitemapService
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly ISitemapPageTypeRepository _sitemapPageTypeRepository;
        private readonly ISitemapContentRepository _sitemapContentRepository;
        private readonly IEventAggregator _eventAggregator;

        public SitemapService(IContentTypeService contentTypeService, ISitemapPageTypeRepository sitemapPageTypeRepository, ISitemapContentRepository sitemapContentRepository, IEventAggregator eventAggregator)
        {
            _contentTypeService = contentTypeService;
            _sitemapPageTypeRepository = sitemapPageTypeRepository;
            _sitemapContentRepository = sitemapContentRepository;
            _eventAggregator = eventAggregator;
        }

        public void SetPageTypeSettings(SitemapPageSettings pageSettings)
        {
            var contentType = _contentTypeService.Get(pageSettings.ContentTypeGuid);
            if (contentType is null) return;
            if (contentType.IsElement) // We don't allow setting this on elements
            {
                throw new ArgumentException("Sitemap settings cannot be set on element document types");
            }

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

        public void SetContentSettings(SitemapContentSettings settings)
        {
            var isDefault = !settings.ExcludeFromSitemap && string.IsNullOrEmpty(settings.ChangeFrequency) && !settings.Priority.HasValue;

            if (isDefault)
            {
                _sitemapContentRepository.Delete(settings.NodeKey);
                return;
            }

            _sitemapContentRepository.Set(settings);
        }

        public SitemapContentSettings? GetContentSettings(Guid nodeKey)
        {
            return _sitemapContentRepository.Get(nodeKey);
        }

        public SitemapContentSettings[] GetAllContentSettings()
        {
            return _sitemapContentRepository.GetAll();
        }
    }
}
