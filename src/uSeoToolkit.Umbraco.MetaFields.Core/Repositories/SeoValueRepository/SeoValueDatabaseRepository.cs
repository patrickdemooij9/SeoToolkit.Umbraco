using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository
{
    public class SeoValueDatabaseRepository : ISeoValueRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SeoValueDatabaseRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Add(int nodeId, string fieldAlias, string culture, object value)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Insert(new SeoValueEntity
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
                scope.Database.Update(new SeoValueEntity
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
                    .Where<SeoValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture));
                scope.Complete();
            }
        }

        public bool Exists(int nodeId, string fieldAlias, string culture)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database.FirstOrDefault<SeoValueEntity>(scope.SqlContext.Sql().SelectAll()
                    .From<SeoValueEntity>().Where<SeoValueEntity>(it => it.NodeId == nodeId && it.Alias == fieldAlias && it.Culture == culture)) != null;
            }
        }

        public Dictionary<string, object> GetAllValues(int nodeId, string culture)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database
                    .Fetch<SeoValueEntity>(scope.SqlContext.Sql().Where<SeoValueEntity>(it => it.NodeId == nodeId && it.Culture == culture))
                    .ToDictionary(it => it.Alias, it => JsonConvert.DeserializeObject(it.UserValue));
            }
        }
    }
}
