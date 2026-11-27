using SeoToolkit.Umbraco.Common.Core.Caching;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository
{
    /// <summary>
    /// Caches each domain's key/values, which are read on every page render (e.g. by the title field), and clears
    /// them on every server through <see cref="SeoKeyValuesCacheRefresher"/> when a value is written.
    /// </summary>
    public class CachedSeoKeyValueRepository : ISeoKeyValueRepository
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        private readonly ISeoKeyValueRepository _inner;
        private readonly AppCaches _appCaches;
        private readonly DistributedCache _distributedCache;

        public CachedSeoKeyValueRepository(ISeoKeyValueRepository inner, AppCaches appCaches, DistributedCache distributedCache)
        {
            _inner = inner;
            _appCaches = appCaches;
            _distributedCache = distributedCache;
        }

        public string? Get(string key, Guid? domainId)
        {
            return GetCached(domainId).TryGetValue(key, out var value) ? value : null;
        }

        public Dictionary<string, string> Get(Guid? domainId)
        {
            // A copy, so a caller can't change the cached values.
            return new Dictionary<string, string>(GetCached(domainId));
        }

        public void Set(string key, string value, Guid? domainId)
        {
            _inner.Set(key, value, domainId);
            Refresh(domainId);
        }

        public void Delete(string key, Guid? domainId)
        {
            _inner.Delete(key, domainId);
            Refresh(domainId);
        }

        private IReadOnlyDictionary<string, string> GetCached(Guid? domainId)
        {
            return _appCaches.RuntimeCache.GetCacheItem<IReadOnlyDictionary<string, string>>(
                       SeoKeyValuesCacheRefresher.CacheKey(domainId),
                       () => _inner.Get(domainId),
                       CacheDuration)
                   ?? _inner.Get(domainId);
        }

        private void Refresh(Guid? domainId)
        {
            // DistributedCache ignores an empty id, so the root values have no id of their own to refresh by.
            if (domainId.HasValue)
            {
                _distributedCache.Refresh(SeoKeyValuesCacheRefresher.CacheGuid, domainId.Value);
            }
            else
            {
                _distributedCache.RefreshAll(SeoKeyValuesCacheRefresher.CacheGuid);
            }
        }
    }
}
