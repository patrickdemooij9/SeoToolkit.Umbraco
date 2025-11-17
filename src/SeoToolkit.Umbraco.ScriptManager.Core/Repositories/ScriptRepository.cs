using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Repositories
{
    public class ScriptRepository : IScriptRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ScriptDefinitionCollection _scriptDefinitionCollection;

        public ScriptRepository(IScopeProvider scopeProvider, ScriptDefinitionCollection scriptDefinitionCollection)
        {
            _scopeProvider = scopeProvider;
            _scriptDefinitionCollection = scriptDefinitionCollection;
        }

        public Script Add(Script script)
        {
            using var scope = _scopeProvider.CreateScope();
            var entity = ToEntity(script);
            scope.Database.Insert(entity);
            scope.Complete();

            return Get(entity.Id);
        }

        public Script Update(Script script)
        {
            using var scope = _scopeProvider.CreateScope();
            var entity = ToEntity(script);
            scope.Database.Update(entity);
            scope.Complete();

            return Get(entity.Id);
        }

        public void Delete(Script script)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Delete(ToEntity(script));
            scope.Complete();
        }

        public Script Get(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return ToModel(scope.Database.FirstOrDefault<ScriptEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<ScriptEntity>()
                .Where<ScriptEntity>(it => it.Id == id)));
        }

        public Script Get(Guid id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return ToModel(scope.Database.FirstOrDefault<ScriptEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<ScriptEntity>()
                .Where<ScriptEntity>(it => it.Key == id)));
        }

        public IEnumerable<Script> GetAll(Guid? domainId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var sql = scope.SqlContext.Sql()
                .SelectAll()
                .From<ScriptEntity>();
            if (domainId.HasValue)
            {
                sql = sql.Where<ScriptEntity>(it => it.DomainId == domainId.Value);
            }
            else
            {
                sql = sql.Where<ScriptEntity>(it => it.DomainId == null);
            }
            return scope.Database.Fetch<ScriptEntity>(sql).Select(ToModel);
        }

        //TODO: Probably move to a mapper
        private ScriptEntity ToEntity(Script script)
        {
            return new ScriptEntity
            {
                Id = script.Id,
                Key = script.Key.Value,
                Name = script.Name,
                DefinitionAlias = script.Definition?.Alias,
                Config = JsonSerializer.Serialize(script.Config),
                DomainId = script.DomainId
            };
        }

        private Script ToModel(ScriptEntity entity)
        {
            return new Script
            {
                Id = entity.Id,
                Key = entity.Key,
                Name = entity.Name,
                Definition = _scriptDefinitionCollection.Get(entity.DefinitionAlias),
                Config = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.Config),
                DomainId = entity.DomainId
            };
        }
    }
}
