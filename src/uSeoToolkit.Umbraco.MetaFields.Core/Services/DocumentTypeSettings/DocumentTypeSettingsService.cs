using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings
{
    public class DocumentTypeSettingsService : IDocumentTypeSettingsService
    {
        private const string BaseCacheKey = "DocumentTypeSettings_";

        private readonly IRepository<DocumentTypeSettingsDto> _repository;
        private readonly FieldProviderCollection _fieldProviders;
        private readonly IAppPolicyCache _cache;

        public DocumentTypeSettingsService(IRepository<DocumentTypeSettingsDto> repository,
            FieldProviderCollection fieldProviders,
            AppCaches appCaches)
        {
            _repository = repository;
            _fieldProviders = fieldProviders;
            _cache = appCaches.RuntimeCache;
        }

        public void Set(DocumentTypeSettingsDto model)
        {
            var exists = _repository.Get(model.Content.Id) != null;
            if (exists)
            {
                var returnModel = _repository.Update(model);
                ClearCache(returnModel.Content.Id);
            }
            else
                _repository.Add(model);
        }

        public DocumentTypeSettingsDto Get(int id)
        {
            return _cache.GetCacheItem($"{BaseCacheKey}{id}_Get", () => _repository.Get(id), TimeSpan.FromMinutes(30));
        }

        public IEnumerable<FieldItemViewModel> GetAdditionalFieldItems()
        {
            return _fieldProviders.GetAllItems();
        }

        public bool IsEnabled(IContent content)
        {
            return Get(content.ContentTypeId)?.EnableSeoSettings is true;
        }

        private void ClearCache(int id)
        {
            _cache.ClearByKey($"{BaseCacheKey}{id}");
        }
    }
}
