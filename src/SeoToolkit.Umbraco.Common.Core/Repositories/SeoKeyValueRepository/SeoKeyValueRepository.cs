using System;
using SeoToolkit.Umbraco.Common.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository
{
    public class SeoKeyValueRepository : ISeoKeyValueRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SeoKeyValueRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public string? Get(string key, Guid? domainId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            var sql = scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoKeyValueEntity>()
                .Where<SeoKeyValueEntity>(it => it.Key == key);
            if (domainId == null)
            {
                sql = sql.Where<SeoKeyValueEntity>(it => it.DomainId == null);
            }
            else
            {
                sql = sql.Where<SeoKeyValueEntity>(it => it.DomainId == domainId);
            }

            var entity = scope.Database.FirstOrDefault<SeoKeyValueEntity>(sql);
            return entity?.Value;
        }

        public void Set(string key, string value, Guid? domainId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            var sql = scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoKeyValueEntity>()
                .Where<SeoKeyValueEntity>(it => it.Key == key);
            if (!domainId.HasValue)
            {
                sql = sql.Where<SeoKeyValueEntity>(it => it.DomainId == null);
            }
            else
            {
                sql = sql.Where<SeoKeyValueEntity>(it => it.DomainId == domainId);
            }
            var existingEntity = scope.Database.FirstOrDefault<SeoKeyValueEntity>(sql);

            existingEntity ??= new SeoKeyValueEntity
            {
                Id = Guid.NewGuid(),
                Key = key,
                DomainId = domainId
            };

            existingEntity.Value = value;

            scope.Database.Save(existingEntity);
        }

        public void Delete(string key, Guid? domainId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);

            var sql = scope.SqlContext.Sql()
                .SelectAll()
                .From<SeoKeyValueEntity>()
                .Where<SeoKeyValueEntity>(it => it.Key == key);
            if (!domainId.HasValue)
            {
                sql = sql.Where<SeoKeyValueEntity>(it => it.DomainId == null);
            }
            else
            {
                sql = sql.Where<SeoKeyValueEntity>(it => it.DomainId == domainId);
            }
            var existingEntity = scope.Database.FirstOrDefault<SeoKeyValueEntity>(sql);

            if (existingEntity is null) return;

            scope.Database.Delete(existingEntity);
        }
    }
}
