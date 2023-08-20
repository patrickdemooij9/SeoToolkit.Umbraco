using System;
using SeoToolkit.Umbraco.Common.Core.Caching;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public class SeoSettingsService : ISeoSettingsService
    {
        private readonly ISeoSettingsRepository _seoSettingsRepository;
        private readonly DistributedCache _distributedCache;
        private readonly IAppPolicyCache _cache;

        public SeoSettingsService(ISeoSettingsRepository seoSettingsRepository, AppCaches appCaches, DistributedCache distributedCache)
        {
            _seoSettingsRepository = seoSettingsRepository;
            _distributedCache = distributedCache;
            _cache = appCaches.RuntimeCache;
        }

        public bool IsEnabled(int contentTypeId)
        {
            return _cache.GetCacheItem($"{CacheConstants.SeoSettings}{contentTypeId}",
                () => _seoSettingsRepository.IsEnabled(contentTypeId), TimeSpan.FromMinutes(10));
        }

        public void ToggleSeoSettings(int contentTypeId, bool value)
        {
            _seoSettingsRepository.Toggle(contentTypeId, value);

            _distributedCache.Refresh(SeoSettingsCacheRefresher.CacheGuid, contentTypeId);
        }
    }
}
