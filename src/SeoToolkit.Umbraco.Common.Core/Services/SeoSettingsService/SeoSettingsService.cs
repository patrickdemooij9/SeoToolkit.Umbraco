using SeoToolkit.Umbraco.Common.Core.Caching;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public class SeoSettingsService : ISeoSettingsService
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly ISeoSettingsRepository _seoSettingsRepository;
        private readonly DistributedCache _distributedCache;
        private readonly AppCaches _cache;
        private readonly ISettingsService<GlobalConfig> _settingsService;
        private readonly IEventAggregator _eventAggregator;

        public SeoSettingsService(
            IContentTypeService contentTypeService,
            ISeoSettingsRepository seoSettingsRepository,
            AppCaches appCaches,
            DistributedCache distributedCache,
            ISettingsService<GlobalConfig> settingsService,
            IEventAggregator eventAggregator)
        {
            _contentTypeService = contentTypeService;
            _seoSettingsRepository = seoSettingsRepository;
            _distributedCache = distributedCache;
            _settingsService = settingsService;
            _eventAggregator = eventAggregator;
            _cache = appCaches;
        }

        public bool IsEnabled(IContentType contentType)
        {
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.SeoSettings}{contentType.Key}",
                () => _seoSettingsRepository.IsEnabled(contentType), TimeSpan.FromMinutes(10));
        }

        public bool SupressContentAppSavingNotification()
        {
            return _settingsService.GetSettings().SupressContentAppSavingNotification;
        }

        public void ToggleSeoSettings(Guid contentTypeId, bool value)
        {
            var contentType = _contentTypeService.Get(contentTypeId);
            if (contentType is null) return;
            if (contentType.IsElement) // We don't allow setting this on elements
            {
                throw new ArgumentException("Seo settings cannot be set on element document types");
            }

            _seoSettingsRepository.Toggle(contentTypeId, value);

            _distributedCache.Refresh(SeoSettingsCacheRefresher.CacheGuid, contentTypeId);
            _eventAggregator.Publish(new SeoSettingSavedNotification(contentTypeId, value));
        }

        public Dictionary<Guid, bool> GetAll()
        {
            return _seoSettingsRepository.GetAll();
        }
    }
}
