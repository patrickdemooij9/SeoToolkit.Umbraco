using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;

namespace SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository
{
    public class MetaFieldsDatabaseRepository : IMetaFieldsValueRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public MetaFieldsDatabaseRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Add(int nodeId, string fieldAlias, string culture, object value)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Insert(new MetaFieldsValueEntity
                {
                    NodeId = nodeId,
                    Alias = fieldAlias,
                    Culture = culture,
                    UserValue = JsonConvert.SerializeObject(value)
                });
                scope.Complete();
            }
        }

        public void Update(int nodeId, string fieldAlias, string culture, object value)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Update(new MetaFieldsValueEntity
                {
                    NodeId = nodeId,
                    Alias = fieldAlias,
                    Culture = culture,
                    UserValue = JsonConvert.SerializeObject(value)
                });
                scope.Complete();
            }
        }

        public void Delete(int nodeId, string fieldAlias, string culture)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Delete(scope.SqlContext.Sql()
                    .Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture));
                scope.Complete();
            }
        }

        public bool Exists(int nodeId, string fieldAlias, string culture)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database.FirstOrDefault<MetaFieldsValueEntity>(scope.SqlContext.Sql().SelectAll()
                    .From<MetaFieldsValueEntity>().Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture)) != null;
            }
        }

        public Dictionary<string, object> GetAllValues(int nodeId, string culture)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database
                    .Fetch<MetaFieldsValueEntity>(scope.SqlContext.Sql().Where<MetaFieldsValueEntity>(it => it.NodeId == nodeId && it.Culture == culture))
                    .ToDictionary(it => it.Alias, it => JsonConvert.DeserializeObject(it.UserValue));
            }
        }
    }
}
