using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Caching;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.DocumentTypeSettingsRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings
{
    public class MetaFieldsSettingsService : IMetaFieldsSettingsService
    {
        private readonly IMetaFieldsSettingsRepository _repository;
        private readonly FieldProviderCollection _fieldProviders;
        private readonly DistributedCache _distributedCache;
        private readonly IEventAggregator _eventAggregator;
        private readonly AppCaches _cache;

        public MetaFieldsSettingsService(IMetaFieldsSettingsRepository repository,
            FieldProviderCollection fieldProviders,
            AppCaches appCaches,
            DistributedCache distributedCache,
            IEventAggregator eventAggregator)
        {
            _repository = repository;
            _fieldProviders = fieldProviders;
            _distributedCache = distributedCache;
            _eventAggregator = eventAggregator;
            _cache = appCaches;
        }

        public void Set(DocumentTypeSettingsDto model)
        {
            if (model.Content.IsElement) // We don't allow setting this on elements
            {
                throw new ArgumentException("MetaFields cannot be set on element document types");
            }

            var exists = _repository.Get(model.Content.Key) != null;
            if (exists)
                _repository.Update(model);
            else
                _repository.Add(model);

            ClearCache(model.Content.Key);
            _eventAggregator.Publish(new MetaFieldSettingsSavedNotification(model));
        }

        public DocumentTypeSettingsDto Get(int id)
        {
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.DocumentTypeSettings}{id}_Get", () =>
            {
                return new CachedNullableModel<DocumentTypeSettingsDto>(_repository.Get(id));
            }, TimeSpan.FromMinutes(30)).Model;
        }

        public DocumentTypeSettingsDto Get(Guid id)
        {
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.DocumentTypeSettings}{id}_Get", () =>
            {
                return new CachedNullableModel<DocumentTypeSettingsDto>(_repository.Get(id));
            }, TimeSpan.FromMinutes(30)).Model;
        }

        public DocumentTypeSettingsDto[] GetAll()
        {
            return _repository.GetAll().ToArray();
        }

        public IEnumerable<FieldItemViewModel> GetAdditionalFieldItems()
        {
            return _fieldProviders.GetAllItems();
        }

        public void Delete(Guid contentTypeGuid)
        {
            _repository.Delete(contentTypeGuid);
            ClearCache(contentTypeGuid);
        }

        private void ClearCache(Guid id)
        {
            _distributedCache.Refresh(DocumentTypeSettingsCacheRefresher.CacheGuid, id);
        }
    }
}
