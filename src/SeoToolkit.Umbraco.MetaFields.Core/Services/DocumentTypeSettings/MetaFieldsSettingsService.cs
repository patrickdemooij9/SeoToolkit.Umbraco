using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using Microsoft.Extensions.Caching.Distributed;
using SeoToolkit.Umbraco.MetaFields.Core.Caching;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings
{
    public class MetaFieldsSettingsService : IMetaFieldsSettingsService
    {
        private readonly IRepository<DocumentTypeSettingsDto> _repository;
        private readonly FieldProviderCollection _fieldProviders;
        private readonly DistributedCache _distributedCache;
        private readonly IAppPolicyCache _cache;

        public MetaFieldsSettingsService(IRepository<DocumentTypeSettingsDto> repository,
            FieldProviderCollection fieldProviders,
            AppCaches appCaches,
            DistributedCache distributedCache)
        {
            _repository = repository;
            _fieldProviders = fieldProviders;
            _distributedCache = distributedCache;
            _cache = appCaches.RuntimeCache;
        }

        public void Set(DocumentTypeSettingsDto model)
        {
            var exists = _repository.Get(model.Content.Id) != null;
            if (exists)
                _repository.Update(model);
            else
                _repository.Add(model);

            ClearCache(model.Content.Id);
        }

        public DocumentTypeSettingsDto Get(int id)
        {
            return _cache.GetCacheItem($"{CacheConstants.DocumentTypeSettings}{id}_Get", () =>
            {
                return new CachedNullableModel<DocumentTypeSettingsDto>(_repository.Get(id));
            }, TimeSpan.FromMinutes(30)).Model;
        }

        public IEnumerable<FieldItemViewModel> GetAdditionalFieldItems()
        {
            return _fieldProviders.GetAllItems();
        }

        private void ClearCache(int id)
        {
            _distributedCache.Refresh(DocumentTypeSettingsCacheRefresher.CacheGuid, id);
        }
    }
}
