using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Models.Database;
using System;
using System.Linq;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.Domains
{
    public class SeoDomainsRepository : ISeoDomainsRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SeoDomainsRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public SeoDomainCollection? Get(Guid id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var collection = scope.Database.FirstOrDefault<SeoDomainCollectionEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainCollectionEntity>()
                .Where<SeoDomainCollectionEntity>(it => it.Id == id));
            if (collection is null) return null;

            var domains = scope.Database.Fetch<SeoDomainEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainEntity>()
                .Where<SeoDomainEntity>(it => it.CollectionId == collection.Id)).Select(x => x.DomainId).ToList();
            var settings = scope.Database.Fetch<SeoDomainSettingEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainSettingEntity>()
                .Where<SeoDomainSettingEntity>(it => it.CollectionId == collection.Id)).ToDictionary(x => x.Key, x => x.Value);

            return new SeoDomainCollection
            {
                Name = collection.Name,
                BaseUrl = collection.BaseUrl,
                Id = collection.Id,
                DomainIds = domains,
                Settings = settings
            };
        }

        public SeoDomainCollection[] GetAll()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var collections = scope.Database.Fetch<SeoDomainCollectionEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainCollectionEntity>()).Select(it => new SeoDomainCollection
                {
                    Id = it.Id,
                    Name = it.Name,
                    BaseUrl = it.BaseUrl
                }).ToArray();

            var domains = scope.Database.Fetch<SeoDomainEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainEntity>()).GroupBy(it => it.CollectionId);
            var settings = scope.Database.Fetch<SeoDomainSettingEntity>(scope.SqlContext.Sql().SelectAll()
                .From<SeoDomainSettingEntity>()).GroupBy(it => it.CollectionId);

            foreach (var collection in collections)
            {
                collection.DomainIds = domains.FirstOrDefault(it => it.Key == collection.Id)?.Select(x => x.DomainId).ToList() ?? [];
                collection.Settings = settings.FirstOrDefault(it => it.Key == collection.Id)?.ToDictionary(x => x.Key, x => x.Value) ?? [];
            }
            return collections;
        }

        public Guid Save(SeoDomainCollection collection)
        {
            using var scope = _scopeProvider.CreateScope();

            SeoDomainCollectionEntity? collectionEntity = null;
            if (collection.Id.HasValue)
            {
                collectionEntity = scope.Database.FirstOrDefault<SeoDomainCollectionEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SeoDomainCollectionEntity>()
                    .Where<SeoDomainCollectionEntity>(it => it.Id == collection.Id));
            }

            collectionEntity ??= new SeoDomainCollectionEntity
            {
                // Preserve a caller-supplied Id when it doesn't exist yet in this environment
                // (e.g. a collection transferred via Deploy) so the identifier round-trips.
                Id = collection.Id == null || collection.Id.Value == Guid.Empty ? Guid.NewGuid() : collection.Id.Value
            };

            collectionEntity.Name = collection.Name;
            collectionEntity.BaseUrl = collection.BaseUrl;
            scope.Database.Save(collectionEntity);

            var existingDomains = scope.Database.Fetch<SeoDomainEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainEntity>()
                .Where<SeoDomainEntity>(it => it.CollectionId == collectionEntity.Id))
                .ToDictionary(x => x.DomainId, x => x);
            foreach (var domainId in collection.DomainIds)
            {
                if (existingDomains.ContainsKey(domainId)) continue;

                var domainEntity = new SeoDomainEntity
                {
                    Id = Guid.NewGuid(),
                    DomainId = domainId,
                    CollectionId = collectionEntity.Id
                };
                scope.Database.Save(domainEntity);
            }
            foreach (var existingDomain in existingDomains.Values)
            {
                if (!collection.DomainIds.Contains(existingDomain.DomainId))
                {
                    scope.Database.Delete(existingDomain);
                }
            }

            var existingSettings = scope.Database.Fetch<SeoDomainSettingEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainSettingEntity>()
                .Where<SeoDomainSettingEntity>(it => it.CollectionId == collectionEntity.Id))
                .ToDictionary(x => x.Key, x => x);
            foreach (var setting in collection.Settings)
            {
                var settingEntity = existingSettings.TryGetValue(setting.Key, out var value) ? value : new SeoDomainSettingEntity { Id = Guid.NewGuid(), Key = setting.Key, CollectionId = collectionEntity.Id };

                settingEntity.Value = setting.Value;
                scope.Database.Save(settingEntity);
            }
            foreach (var existingSetting in existingSettings.Values)
            {
                if (!collection.Settings.ContainsKey(existingSetting.Key))
                {
                    scope.Database.Delete(existingSetting);
                }
            }

            scope.Complete();
            return collectionEntity.Id;
        }

        public void Delete(Guid domainId)
        {
            using var scope = _scopeProvider.CreateScope();

            var entity = scope.Database.FirstOrDefault<SeoDomainCollectionEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainCollectionEntity>()
                .Where<SeoDomainCollectionEntity>(it => it.Id == domainId));
            if (entity is null)
            {
                return;
            }

            var domainEntities = scope.Database.Fetch<SeoDomainEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainEntity>()
                .Where<SeoDomainEntity>(it => it.CollectionId == domainId));
            var settingEntities = scope.Database.Fetch<SeoDomainSettingEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainSettingEntity>()
                .Where<SeoDomainSettingEntity>(it => it.CollectionId == domainId));

            foreach (var domainEntity in domainEntities)
            {
                scope.Database.Delete(domainEntity);
            }
            foreach (var settingEntity in settingEntities)
            {
                scope.Database.Delete(settingEntity);
            }
            scope.Database.Delete(entity);

            scope.Complete();
        }
    }
}
