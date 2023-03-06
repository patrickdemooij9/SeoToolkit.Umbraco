using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService
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

        public Dictionary<string, object> GetUserValues(int nodeId, string culture = null)
        {
            var foundCulture = culture.IfNullOrWhiteSpace(GetCulture());
            return _cache.GetCacheItem($"{BaseCacheKey}{nodeId}_{foundCulture}", () => _repository.GetAllValues(nodeId, foundCulture), TimeSpan.FromMinutes(30));
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
