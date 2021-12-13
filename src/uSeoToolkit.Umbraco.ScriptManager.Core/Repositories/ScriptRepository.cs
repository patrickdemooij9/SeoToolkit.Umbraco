using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.ScriptManager.Core.Collections;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Database;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Repositories
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

        public void Add(Script script)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Insert(ToEntity(script));
                scope.Complete();
            }
        }

        public void Update(Script script)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Update(ToEntity(script));
                scope.Complete();
            }
        }

        public void Delete(Script script)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Delete(ToEntity(script));
                scope.Complete();
            }
        }

        public Script Get(int id)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return ToModel(scope.Database.FirstOrDefault<ScriptEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<ScriptEntity>()
                    .Where<ScriptEntity>(it => it.Id == id)));
            }
        }

        public IEnumerable<Script> GetAll()
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return scope.Database.Fetch<ScriptEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<ScriptEntity>()).Select(ToModel);
            }
        }

        //TODO: Probably move to a mapper
        private ScriptEntity ToEntity(Script script)
        {
            return new ScriptEntity
            {
                Id = script.Id,
                Name = script.Name,
                DefinitionAlias = script.Definition?.Alias,
                Config = JsonSerializer.Serialize(script.Config)
            };
        }

        private Script ToModel(ScriptEntity entity)
        {
            return new Script
            {
                Id = entity.Id,
                Name = entity.Name,
                Definition = _scriptDefinitionCollection.Get(entity.DefinitionAlias),
                Config = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Config)
            };
        }
    }
}
