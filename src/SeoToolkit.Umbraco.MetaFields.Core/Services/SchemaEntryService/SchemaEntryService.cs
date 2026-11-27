using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Caching;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SchemaEntryRepository;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService
{
    public class SchemaEntryService : ISchemaEntryService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        private readonly ISchemaEntryRepository _repository;
        private readonly AppCaches _appCaches;
        private readonly DistributedCache _distributedCache;

        public SchemaEntryService(ISchemaEntryRepository repository, AppCaches appCaches, DistributedCache distributedCache)
        {
            _repository = repository;
            _appCaches = appCaches;
            _distributedCache = distributedCache;
        }

        public IEnumerable<SchemaEntryDto> GetAll(string ownerType, Guid ownerKey)
        {
            return _appCaches.RuntimeCache.GetCacheItem(
                $"{CacheConstants.SchemaEntry}All_{ownerType}_{ownerKey}",
                () => _repository.GetAll(ownerType, ownerKey).ToList(),
                CacheDuration);
        }

        public SchemaEntryDto GetById(Guid id)
        {
            // The runtime cache doesn't store null, so a missing entry is cached wrapped; otherwise a page that
            // references a deleted entry queries the database on every render.
            return _appCaches.RuntimeCache.GetCacheItem(
                $"{CacheConstants.SchemaEntry}{id}",
                () => new CachedSchemaEntry(_repository.GetById(id)),
                CacheDuration)?.Entry;
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
            RefreshCache();
            return result;
        }

        public SchemaEntryDto Update(SchemaEntryDto model)
        {
            var result = _repository.Update(model);
            RefreshCache();
            return result;
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
            RefreshCache();
        }

        private void RefreshCache()
        {
            _distributedCache.RefreshAll(SchemaEntryCacheRefresher.CacheGuid);
        }

        private sealed record CachedSchemaEntry(SchemaEntryDto Entry);
    }
}
