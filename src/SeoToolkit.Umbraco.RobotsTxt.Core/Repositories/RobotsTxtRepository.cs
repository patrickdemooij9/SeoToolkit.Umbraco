using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Repositories
{
    public class RobotsTxtRepository : IRobotsTxtRepository
    {
        private IScopeProvider _scopeProvider;

        public RobotsTxtRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public RobotsTxtModel Add(RobotsTxtModel model)
        {
            return Update(model);
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Get(Guid key) method instead. Will be removed in next major version.")]
        public RobotsTxtModel Get(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<RobotsTxtEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<RobotsTxtEntity>()
                .Where<RobotsTxtEntity>(it => it.Id == id));
            return entity is null ? null : MapToModel(entity);
        }

        public RobotsTxtModel Get(Guid key)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<RobotsTxtEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<RobotsTxtEntity>()
                .Where<RobotsTxtEntity>(it => it.Key == key));
            return entity is null ? null : MapToModel(entity);
        }

        public IEnumerable<RobotsTxtModel> GetAll()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database.Fetch<RobotsTxtEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<RobotsTxtEntity>()).Select(MapToModel);
        }

        public RobotsTxtModel Update(RobotsTxtModel model)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                scope.Database.Save(MapToEntity(model));
            }
            return Get(model.Key);
        }

        private RobotsTxtModel MapToModel(RobotsTxtEntity entity)
        {
            return new RobotsTxtModel
            {
                Id = entity.Id,
                Key = entity.Key,
                Content = entity.Content,
                DomainId = entity.DomainId
            };
        }

        private RobotsTxtEntity MapToEntity(RobotsTxtModel model)
        {
            return new RobotsTxtEntity
            {
                Id = model.Id,
                Key = model.Key,
                Content = model.Content,
                DomainId = model.DomainId
            };
        }
    }
}
