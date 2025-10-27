using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Models.Database;
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

        public SeoDomainCollection? Get(int id)
        {
            using var scope = _scopeProvider.CreateScope();
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
                Id = collection.Id,
                DomainIds = domains,
                Settings = settings
            };
        }

        public SeoDomainCollection[] GetAll()
        {
            using var scope = _scopeProvider.CreateScope();
            var collections = scope.Database.Fetch<SeoDomainCollectionEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainCollectionEntity>()).Select(it => new SeoDomainCollection
                {
                    Id = it.Id,
                    Name = it.Name
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

        public int Save(SeoDomainCollection collection)
        {
            using var scope = _scopeProvider.CreateScope();

            SeoDomainCollectionEntity collectionEntity;
            if (collection.Id != 0)
            {
                collectionEntity = scope.Database.FirstOrDefault<SeoDomainCollectionEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SeoDomainCollectionEntity>()
                    .Where<SeoDomainCollectionEntity>(it => it.Id == collection.Id));
            }
            else
            {
                collectionEntity = new SeoDomainCollectionEntity();
            }

            collectionEntity.Name = collection.Name;
            scope.Database.Save(collectionEntity);

            var existingDomains = scope.Database.Fetch<SeoDomainEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoDomainEntity>()
                .Where<SeoDomainEntity>(it => it.CollectionId == collectionEntity.Id))
                .ToDictionary(x => x.Id, x => x);
            foreach (var domainId in collection.DomainIds)
            {
                if (existingDomains.ContainsKey(domainId)) continue;

                var domainEntity = new SeoDomainEntity
                {
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
                var settingEntity = existingSettings.TryGetValue(setting.Key, out var value) ? value : new SeoDomainSettingEntity { Key = setting.Key, CollectionId = collectionEntity.Id };

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
    }
}
