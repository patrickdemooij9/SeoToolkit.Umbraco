using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using System;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository
{
    public class MetaFieldsDatabaseRepository : IMetaFieldsValueRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public MetaFieldsDatabaseRepository(IScopeProvider scopeProvider, IUmbracoContextFactory umbracoContextFactory)
        {
            _scopeProvider = scopeProvider;
            _umbracoContextFactory = umbracoContextFactory;
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
            scope.Database.Delete(scope.SqlContext.Sql()
                .Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture));
            scope.Complete();
        }

        public bool Exists(int nodeId, string fieldAlias, string culture)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database.FirstOrDefault<MetaFieldsValueEntity>(scope.SqlContext.Sql().SelectAll()
                .From<MetaFieldsValueEntity>().Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture)) != null;
        }

        public Dictionary<string, object> GetAllValues(int nodeId, string culture)
        {
            using var scope = _scopeProvider.CreateScope();
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
            scope.Database.Delete(scope.SqlContext.Sql()
                .Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId && it.Alias == fieldAlias && it.Culture == culture));
            scope.Complete();
        }

        public bool Exists(Guid nodeId, string fieldAlias, string culture)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database.FirstOrDefault<MetaFieldsValueEntity>(scope.SqlContext.Sql().SelectAll()
                .From<MetaFieldsValueEntity>().Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId && it.Alias == fieldAlias && it.Culture == culture)) != null;
        }

        public Dictionary<string, object> GetAllValues(Guid nodeId, string culture)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database
                .Fetch<MetaFieldsValueEntity>(scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(it => it.NodeKey == nodeId && it.Culture == culture))
                .ToDictionary(it => it.Alias, it => JsonConvert.DeserializeObject(it.UserValue));
        }

        public IEnumerable<(Guid NodeKey, string UserValue)> GetAllValuesByFieldAlias(string fieldAlias, string culture)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database
                .Fetch<MetaFieldsValueEntity>(scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(it => it.Alias == fieldAlias && it.Culture == culture))
                .Select(it => (it.NodeKey, it.UserValue))
                .ToList();
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
            using var umbContextRef = _umbracoContextFactory.EnsureUmbracoContext();
            var content = umbContextRef.UmbracoContext.Content.GetById(nodeKey);
            return content?.Id ?? 0;
        }
    }
}
