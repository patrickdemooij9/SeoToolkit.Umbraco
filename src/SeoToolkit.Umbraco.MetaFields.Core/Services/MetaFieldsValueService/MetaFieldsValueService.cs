using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using Microsoft.Extensions.Caching.Distributed;
using SeoToolkit.Umbraco.MetaFields.Core.Caching;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService
{
    public class MetaFieldsValueService : IMetaFieldsValueService
    {
        private readonly IMetaFieldsValueRepository _repository;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly DistributedCache _distributedCache;
        private readonly AppCaches _cache;
        private readonly IEventAggregator _eventAggregator;

        public MetaFieldsValueService(IMetaFieldsValueRepository repository, IVariationContextAccessor variationContextAccessor, AppCaches appCaches, DistributedCache distributedCache, IEventAggregator eventAggregator)
        {
            _repository = repository;
            _variationContextAccessor = variationContextAccessor;
            _distributedCache = distributedCache;
            _cache = appCaches;
            _eventAggregator = eventAggregator;
        }

        public Dictionary<string, object> GetUserValues(int nodeId, string culture = null)
        {
            var foundCulture = culture.IfNullOrWhiteSpace(GetCulture());
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.SeoValue}{nodeId}_{foundCulture}", () => _repository.GetAllValues(nodeId, foundCulture), TimeSpan.FromMinutes(30));
        }

        public void AddValues(int nodeId, Dictionary<string, object> values, string culture = null)
        {
            var foundCulture = culture.IfNullOrWhiteSpace(GetCulture());
            foreach (var (key, value) in values)
            {
                if (_repository.Exists(nodeId, key, foundCulture))
                    _repository.Update(nodeId, key, foundCulture, value);
                else
                {
                    _repository.Add(nodeId, key, foundCulture, value);
                }
            }
            ClearCache(nodeId);
        }
        
        public void Delete(int nodeId, string fieldAlias, string culture = null)
        {
            _repository.Delete(nodeId, fieldAlias, culture.IfNullOrWhiteSpace(GetCulture()));
            ClearCache(nodeId);
        }

        public void Delete(Guid nodeId, string fieldAlias, string culture = null)
        {
            _repository.Delete(nodeId, fieldAlias, culture.IfNullOrWhiteSpace(GetCulture()));
            ClearCache(nodeId);
            _eventAggregator.Publish(new MetaFieldsValueChangedNotification(nodeId));
        }

        public Dictionary<string, object> GetUserValues(Guid nodeId, string culture = null)
        {
            var foundCulture = culture.IfNullOrWhiteSpace(GetCulture());
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.SeoValue}{nodeId}_{foundCulture}", () => _repository.GetAllValues(nodeId, foundCulture), TimeSpan.FromMinutes(30));
        }

        public void AddValues(Guid nodeId, Dictionary<string, object> values, string culture = null)
        {
            var foundCulture = culture.IfNullOrWhiteSpace(GetCulture());
            foreach (var (key, value) in values)
            {
                if (_repository.Exists(nodeId, key, foundCulture))
                    _repository.Update(nodeId, key, foundCulture, value);
                else
                {
                    _repository.Add(nodeId, key, foundCulture, value);
                }
            }
            ClearCache(nodeId);
            _eventAggregator.Publish(new MetaFieldsValueChangedNotification(nodeId));
        }


        public void NotifyChanged(Guid nodeId)
        {
            ClearCache(nodeId);
            _eventAggregator.Publish(new MetaFieldsValueChangedNotification(nodeId));
        }

        private string GetCulture()
        {
            return _variationContextAccessor.VariationContext.Culture;
        }

        private void ClearCache(int nodeId)
        {
            _distributedCache.Refresh(SeoValueCacheRefresher.CacheGuid, nodeId);
        }

        private void ClearCache(Guid nodeId)
        {
            _distributedCache.Refresh(SeoValueCacheRefresher.CacheGuid, nodeId);
        }
    }
}
