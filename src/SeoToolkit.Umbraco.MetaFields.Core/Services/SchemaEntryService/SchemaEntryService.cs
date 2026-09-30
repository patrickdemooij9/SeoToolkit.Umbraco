using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SchemaEntryRepository;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService
{
    public class SchemaEntryService : ISchemaEntryService
    {
        private readonly ISchemaEntryRepository _repository;
        private readonly AppCaches _appCaches;

        private const string CacheKeyPrefix = "SeoToolkitSchemaEntry_";

        public SchemaEntryService(ISchemaEntryRepository repository, AppCaches appCaches)
        {
            _repository = repository;
            _appCaches = appCaches;
        }

        public IEnumerable<SchemaEntryDto> GetAll(string ownerType, Guid ownerKey)
        {
            return _appCaches.RuntimeCache.GetCacheItem(
                $"{CacheKeyPrefix}All_{ownerType}_{ownerKey}",
                () => _repository.GetAll(ownerType, ownerKey).ToList(),
                TimeSpan.FromMinutes(30));
        }

        public SchemaEntryDto GetById(Guid id)
        {
            return _appCaches.RuntimeCache.GetCacheItem(
                $"{CacheKeyPrefix}{id}",
                () => _repository.GetById(id),
                TimeSpan.FromMinutes(30));
        }

        public IEnumerable<SchemaEntryDto> GetByIds(IEnumerable<Guid> ids)
        {
            return ids.Select(GetById).Where(x => x != null).ToList();
        }

        public SchemaEntryDto Add(SchemaEntryDto model)
        {
            if (model.Id == Guid.Empty)
                model.Id = Guid.NewGuid();

            var result = _repository.Add(model);
            ClearOwnerCache(result.OwnerType, result.OwnerKey);
            return result;
        }

        public SchemaEntryDto Update(SchemaEntryDto model)
        {
            var result = _repository.Update(model);
            _appCaches.RuntimeCache.ClearByKey($"{CacheKeyPrefix}{model.Id}");
            ClearOwnerCache(model.OwnerType, model.OwnerKey);
            return result;
        }

        public void Delete(Guid id)
        {
            var existing = GetById(id);
            _repository.Delete(id);
            _appCaches.RuntimeCache.ClearByKey($"{CacheKeyPrefix}{id}");
            if (existing != null)
                ClearOwnerCache(existing.OwnerType, existing.OwnerKey);
        }

        private void ClearOwnerCache(string ownerType, Guid ownerKey)
        {
            _appCaches.RuntimeCache.ClearByKey($"{CacheKeyPrefix}All_{ownerType}_{ownerKey}");
        }
    }
}
