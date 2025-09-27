using Microsoft.Extensions.Caching.Distributed;
using SeoToolkit.Umbraco.Common.Core.Caching;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Repositories.Domains;
using System;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Services.Domains
{
    public class SeoDomainsService : ISeoDomainsService
    {
        private readonly ISeoDomainsRepository _seoDomainsRepository;
        private readonly DistributedCache _distributedCache;
        private readonly IAppPolicyCache _cache;

        public SeoDomainsService(ISeoDomainsRepository seoDomainsRepository, AppCaches appCaches, DistributedCache distributedCache)
        {
            _seoDomainsRepository = seoDomainsRepository;
            _cache = appCaches.RuntimeCache;
            _distributedCache = distributedCache;
        }

        public SeoDomainCollection[] GetAll()
        {
            return _cache.GetCacheItem($"{CacheConstants.SeoDomains}GetAll", _seoDomainsRepository.GetAll, TimeSpan.FromMinutes(10)) ?? [];
        }

        public SeoDomainCollection? Get(int id)
        {
            return GetAll().FirstOrDefault(it => it.Id == id);
        }

        public SeoDomainCollection? GetByDomain(int umbracoDomainId)
        {
            return GetAll().FirstOrDefault(it => it.DomainIds.Contains(umbracoDomainId));
        }

        public int Save(SeoDomainCollection collection)
        {
            var id =  _seoDomainsRepository.Save(collection);
            _distributedCache.RefreshAll(SeoDomainsCacheRefresher.CacheRefreshGuid);
            return id;
        }
    }
}
