using System;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public class SeoSettingsService : ISeoSettingsService
    {
        private const string BaseCacheKey = "SeoSettingsService_";

        private readonly ISeoSettingsRepository _seoSettingsRepository;
        private readonly IAppPolicyCache _cache;

        public SeoSettingsService(ISeoSettingsRepository seoSettingsRepository, AppCaches appCaches)
        {
            _seoSettingsRepository = seoSettingsRepository;
            _cache = appCaches.RuntimeCache;
        }

        public bool IsEnabled(int contentTypeId)
        {
            return _cache.GetCacheItem($"{BaseCacheKey}{contentTypeId}",
                () => _seoSettingsRepository.IsEnabled(contentTypeId), TimeSpan.FromMinutes(10));
        }

        public void ToggleSeoSettings(int contentTypeId, bool value)
        {
            _seoSettingsRepository.Toggle(contentTypeId, value);

            _cache.ClearByKey($"{BaseCacheKey}{contentTypeId}");
        }
    }
}
