using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Repositories.SchemaEntryRepository
{
    public class SchemaEntryDatabaseRepository : ISchemaEntryRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SchemaEntryDatabaseRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public IEnumerable<SchemaEntryDto> GetAll(string ownerType, Guid ownerKey)
        {
            using var scope = _scopeProvider.CreateScope();
            return scope.Database
                .Fetch<SchemaEntryEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SchemaEntryEntity>()
                    .Where<SchemaEntryEntity>(it => it.OwnerType == ownerType && it.OwnerKey == ownerKey))
                .Select(MapToDto)
                .ToList();
        }

        public SchemaEntryDto GetById(Guid id)
        {
            using var scope = _scopeProvider.CreateScope();
            var entity = scope.Database.FirstOrDefault<SchemaEntryEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SchemaEntryEntity>()
                .Where<SchemaEntryEntity>(it => it.Id == id));
            return entity is null ? null : MapToDto(entity);
        }

        public IEnumerable<SchemaEntryDto> GetByIds(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0)
                return [];

            using var scope = _scopeProvider.CreateScope();
            var entities = scope.Database.Fetch<SchemaEntryEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SchemaEntryEntity>()
                .Where<SchemaEntryEntity>(it => idList.Contains(it.Id)));

            return entities.Select(MapToDto).ToArray();
        }

        public SchemaEntryDto Add(SchemaEntryDto model)
        {
            var entity = MapToEntity(model);
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(entity);
            scope.Complete();
            return model;
        }

        public SchemaEntryDto Update(SchemaEntryDto model)
        {
            var entity = MapToEntity(model);
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Update(entity);
            scope.Complete();
            return model;
        }

        public void Delete(Guid id)
        {
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Execute("DELETE FROM SeoToolkitSchemaEntry WHERE Id = @0", id);
            scope.Complete();
        }

        private static SchemaEntryDto MapToDto(SchemaEntryEntity entity)
        {
            return new SchemaEntryDto
            {
                Id = entity.Id,
                OwnerType = entity.OwnerType,
                OwnerKey = entity.OwnerKey,
                SchemaAlias = entity.SchemaAlias,
                DisplayName = entity.DisplayName,
                RenderAutomatically = entity.RenderAutomatically,
                Properties = string.IsNullOrWhiteSpace(entity.PropertiesJson)
                    ? new Dictionary<string, SchemaPropertyValue>()
                    : JsonConvert.DeserializeObject<Dictionary<string, SchemaPropertyValue>>(entity.PropertiesJson)
                      ?? new Dictionary<string, SchemaPropertyValue>()
            };
        }

        private static SchemaEntryEntity MapToEntity(SchemaEntryDto dto)
        {
            return new SchemaEntryEntity
            {
                Id = dto.Id,
                OwnerType = dto.OwnerType,
                OwnerKey = dto.OwnerKey,
                SchemaAlias = dto.SchemaAlias,
                DisplayName = dto.DisplayName,
                RenderAutomatically = dto.RenderAutomatically,
                PropertiesJson = JsonConvert.SerializeObject(dto.Properties)
            };
        }
    }
}
