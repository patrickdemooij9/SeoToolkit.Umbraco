using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository
{
    public class MetaFieldsDatabaseRepository : IMetaFieldsValueRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IIdKeyMap _idKeyMap;

        public MetaFieldsDatabaseRepository(IScopeProvider scopeProvider, IUmbracoContextFactory umbracoContextFactory, IIdKeyMap idKeyMap)
        {
            _scopeProvider = scopeProvider;
            _umbracoContextFactory = umbracoContextFactory;
            _idKeyMap = idKeyMap;
        }

        public void Add(int nodeId, string fieldAlias, string culture, object value)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(new MetaFieldsValueEntity
            {
                NodeId = nodeId,
                NodeKey = GetNodeKey(nodeId),
                Alias = fieldAlias,
                Culture = culture,
                UserValue = JsonConvert.SerializeObject(value)
            });
            scope.Complete();
        }

        public void Update(int nodeId, string fieldAlias, string culture, object value)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Update(new MetaFieldsValueEntity
            {
                NodeId = nodeId,
                NodeKey = GetNodeKey(nodeId),
                Alias = fieldAlias,
                Culture = culture,
                UserValue = JsonConvert.SerializeObject(value)
            });
            scope.Complete();
        }

        public void Delete(int nodeId, string fieldAlias, string culture)
        {
            using var scope = _scopeProvider.CreateScope();
            // Generic Delete<T>(Sql) builds "DELETE FROM <table> WHERE ...". The non-generic
            // Delete(sql) binds to Delete(object poco) and crashes in NPoco's setter emit.
            scope.Database.Delete<MetaFieldsValueEntity>(scope.SqlContext.Sql()
                .Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture));
            scope.Complete();
        }

        public bool Exists(int nodeId, string fieldAlias, string culture)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.FirstOrDefault<MetaFieldsValueEntity>(scope.SqlContext.Sql().SelectAll()
                .From<MetaFieldsValueEntity>().Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture)) != null;
        }

        public Dictionary<string, object> GetAllValues(int nodeId, string culture)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database
                .Fetch<MetaFieldsValueEntity>(scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Culture == culture))
                .ToDictionary(it => it.Alias, it => JsonConvert.DeserializeObject(it.UserValue));
        }
        
        public void Add(Guid nodeId, string fieldAlias, string culture, object value)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(new MetaFieldsValueEntity
            {
                NodeId = GetNodeId(nodeId),
                NodeKey = nodeId,
                Alias = fieldAlias,
                Culture = culture,
                UserValue = JsonConvert.SerializeObject(value)
            });
            scope.Complete();
        }

        public void Update(Guid nodeId, string fieldAlias, string culture, object value)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Update(new MetaFieldsValueEntity
            {
                NodeId = GetNodeId(nodeId),
                NodeKey = nodeId,
                Alias = fieldAlias,
                Culture = culture,
                UserValue = JsonConvert.SerializeObject(value)
            });
            scope.Complete();
        }

        public void Delete(Guid nodeId, string fieldAlias, string culture)
        {
            using var scope = _scopeProvider.CreateScope();
            // GetAllValues maps a NULL culture to "", so treat an empty culture as "empty or NULL"
            // here — otherwise the prune path (which passes "") could never delete NULL-culture rows.
            var sql = string.IsNullOrEmpty(culture)
                ? scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(
                    it => it.NodeKey == nodeId && it.Alias == fieldAlias && (it.Culture == null || it.Culture == ""))
                : scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(
                    it => it.NodeKey == nodeId && it.Alias == fieldAlias && it.Culture == culture);
            // Generic Delete<T>(Sql) builds "DELETE FROM <table> WHERE ...". The non-generic
            // Delete(sql) binds to Delete(object poco) and crashes in NPoco's setter emit.
            scope.Database.Delete<MetaFieldsValueEntity>(sql);
            scope.Complete();
        }

        public bool Exists(Guid nodeId, string fieldAlias, string culture)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            // Treat an empty culture as "empty or NULL" so a legacy NULL-culture row is matched —
            // otherwise the import path would miss it and insert a duplicate (NodeKey, alias) row.
            var sql = scope.SqlContext.Sql().SelectAll().From<MetaFieldsValueEntity>();
            sql = string.IsNullOrEmpty(culture)
                ? sql.Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId && it.Alias == fieldAlias && (it.Culture == null || it.Culture == ""))
                : sql.Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId && it.Alias == fieldAlias && it.Culture == culture);
            return scope.Database.FirstOrDefault<MetaFieldsValueEntity>(sql) != null;
        }

        public Dictionary<string, object> GetAllValues(Guid nodeId, string culture)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            // Match the NULL-or-empty culture handling used by Exists/Delete for consistency.
            var sql = string.IsNullOrEmpty(culture)
                ? scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId && (it.Culture == null || it.Culture == ""))
                : scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId && it.Culture == culture);
            return scope.Database
                .Fetch<MetaFieldsValueEntity>(sql)
                .ToDictionary(it => it.Alias, it => JsonConvert.DeserializeObject(it.UserValue));
        }

        public Dictionary<string, Dictionary<string, object>> GetAllValues(Guid nodeId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database
                .Fetch<MetaFieldsValueEntity>(scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId))
                .GroupBy(it => it.Culture ?? string.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(it => it.Alias, it => JsonConvert.DeserializeObject(it.UserValue)));
        }

        public IEnumerable<Guid> GetAllNodeKeys()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var sql = scope.SqlContext.Sql()
                .SelectDistinct<MetaFieldsValueEntity>(it => it.NodeKey)
                .From<MetaFieldsValueEntity>();
            return scope.Database.Fetch<Guid>(sql).ToArray();
        }

        public bool HasAnyValues(Guid nodeId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var sql = scope.SqlContext.Sql()
                .SelectCount()
                .From<MetaFieldsValueEntity>()
                .Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId);
            return scope.Database.ExecuteScalar<int>(sql) > 0;
        }

        // These functions should be removed once all int nodeId usages are gone
        private Guid GetNodeKey(int nodeId)
        {
            using var umbContextRef = _umbracoContextFactory.EnsureUmbracoContext();
            var content = umbContextRef.UmbracoContext.Content.GetById(nodeId);
            return content?.Key ?? Guid.Empty;
        }

        private int GetNodeId(Guid nodeKey)
        {
            // Resolve via IIdKeyMap rather than the published content cache: during a deploy the
            // content may exist but be unpublished, which the published cache would resolve to 0.
            var attempt = _idKeyMap.GetIdForKey(nodeKey, UmbracoObjectTypes.Document);
            return attempt.Success ? attempt.Result : 0;
        }
    }
}
