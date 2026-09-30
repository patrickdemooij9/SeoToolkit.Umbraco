using Microsoft.Extensions.Caching.Distributed;
using SeoToolkit.Umbraco.Common.Core.Caching;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Common.Core.Repositories.Domains;
using System;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Services.Domains
{
    public class SeoDomainsService : ISeoDomainsService
    {
        private readonly ISeoDomainsRepository _seoDomainsRepository;
        private readonly DistributedCache _distributedCache;
        private readonly AppCaches _cache;
        private readonly IEventAggregator _eventAggregator;

        public SeoDomainsService(ISeoDomainsRepository seoDomainsRepository, AppCaches appCaches, DistributedCache distributedCache, IEventAggregator eventAggregator)
        {
            _seoDomainsRepository = seoDomainsRepository;
            _cache = appCaches;
            _distributedCache = distributedCache;
            _eventAggregator = eventAggregator;
        }

        public SeoDomainCollection[] GetAll()
        {
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.SeoDomains}GetAll", _seoDomainsRepository.GetAll, TimeSpan.FromMinutes(10)) ?? [];
        }

        public SeoDomainCollection? Get(Guid id)
        {
            return GetAll().FirstOrDefault(it => it.Id == id);
        }

        public SeoDomainCollection? GetByDomain(int umbracoDomainId)
        {
            return GetAll().FirstOrDefault(it => it.DomainIds.Contains(umbracoDomainId));
        }

        public Guid Save(SeoDomainCollection collection)
        {
            var id =  _seoDomainsRepository.Save(collection);
            _distributedCache.RefreshAll(SeoDomainsCacheRefresher.CacheRefreshGuid);
            collection.Id = id;
            _eventAggregator.Publish(new SeoDomainCollectionSavedNotification(collection));
            return id;
        }

        public void Delete(Guid domainId)
        {
            _seoDomainsRepository.Delete(domainId);
            _distributedCache.RefreshAll(SeoDomainsCacheRefresher.CacheRefreshGuid);
            _eventAggregator.Publish(new SeoDomainCollectionDeletedNotification(domainId));
        }
    }
}
