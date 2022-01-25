using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService
{
    public class MetaFieldsValueService : IMetaFieldsValueService
    {
        private const string BaseCacheKey = "SeoValueService_";

        private readonly IMetaFieldsValueRepository _repository;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IAppPolicyCache _cache;

        public MetaFieldsValueService(IMetaFieldsValueRepository repository, IVariationContextAccessor variationContextAccessor, AppCaches appCaches)
        {
            _repository = repository;
            _variationContextAccessor = variationContextAccessor;
            _cache = appCaches.RuntimeCache;
        }

        public Dictionary<string, object> GetUserValues(int nodeId)
        {
            var culture = GetCulture();
            return _cache.GetCacheItem($"{BaseCacheKey}{nodeId}_{culture}", () => _repository.GetAllValues(nodeId, culture), TimeSpan.FromMinutes(30));
        }

        public void AddValues(int nodeId, Dictionary<string, object> values)
        {
            foreach (var (key, value) in values)
            {
                if (_repository.Exists(nodeId, key, GetCulture()))
                    _repository.Update(nodeId, key, GetCulture(), value);
                else
                {
                    _repository.Add(nodeId, key, GetCulture(), value);
                }
            }
            ClearCache(nodeId);
        }

        public void Delete(int nodeId, string fieldAlias)
        {
            _repository.Delete(nodeId, fieldAlias, GetCulture());
        }

        private string GetCulture()
        {
            return _variationContextAccessor.VariationContext.Culture;
        }

        private void ClearCache(int nodeId)
        {
            _cache.ClearByKey($"{BaseCacheKey}{nodeId}");
        }
    }
}
