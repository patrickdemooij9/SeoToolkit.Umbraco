using System;
using SeoToolkit.Umbraco.Common.Core.Caching;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public class SeoSettingsService : ISeoSettingsService
    {
        private readonly ISeoSettingsRepository _seoSettingsRepository;
        private readonly DistributedCache _distributedCache;
        private readonly IAppPolicyCache _cache;
        private readonly ISettingsService<GlobalConfig> _settingsService;

        public SeoSettingsService(ISeoSettingsRepository seoSettingsRepository, AppCaches appCaches, DistributedCache distributedCache, ISettingsService<GlobalConfig> settingsService)
        {
            _seoSettingsRepository = seoSettingsRepository;
            _distributedCache = distributedCache;
            _settingsService = settingsService;
            _cache = appCaches.RuntimeCache;
        }

        public bool IsEnabled(int contentTypeId)
        {
            return _cache.GetCacheItem($"{CacheConstants.SeoSettings}{contentTypeId}",
                () => _seoSettingsRepository.IsEnabled(contentTypeId), TimeSpan.FromMinutes(10));
        }

        public bool SupressContentAppSavingNotification()
        {
            return _settingsService.GetSettings().SupressContentAppSavingNotification;
        }

        public void ToggleSeoSettings(int contentTypeId, bool value)
        {
            _seoSettingsRepository.Toggle(contentTypeId, value);

            _distributedCache.Refresh(SeoSettingsCacheRefresher.CacheGuid, contentTypeId);
        }
    }
}
