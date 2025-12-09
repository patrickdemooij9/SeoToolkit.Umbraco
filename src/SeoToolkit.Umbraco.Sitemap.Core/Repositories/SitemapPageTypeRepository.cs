using System;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;
using System.Linq;

namespace SeoToolkit.Umbraco.Sitemap.Core.Repositories
{
    public class SitemapPageTypeRepository : ISitemapPageTypeRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SitemapPageTypeRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Set(SitemapPageSettings settings)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            scope.Database.Save(new SitemapPageTypeEntity
            {
                ContentTypeId = settings.ContentTypeId,
                ContentTypeGuid = settings.ContentTypeGuid,
                HideFromSitemap = settings.HideFromSitemap,
                ChangeFrequency = settings.ChangeFrequency,
                Priority = settings.Priority
            });
        }

        public SitemapPageSettings Get(int contentTypeId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<SitemapPageTypeEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SitemapPageTypeEntity>()
                .Where<SitemapPageTypeEntity>(it => it.ContentTypeId == contentTypeId));

            return entity is null
                ? null
                : MapFromEntity(entity);
        }

        public SitemapPageSettings Get(Guid contentTypeGuid)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<SitemapPageTypeEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SitemapPageTypeEntity>()
                .Where<SitemapPageTypeEntity>(it => it.ContentTypeGuid == contentTypeGuid));

            return entity is null
                ? null
                : MapFromEntity(entity);
        }

        public SitemapPageSettings[] GetAll()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entities = scope.Database.Fetch<SitemapPageTypeEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SitemapPageTypeEntity>()).ToArray();

            return entities.Select(MapFromEntity).ToArray();
        }

        private SitemapPageSettings MapFromEntity(SitemapPageTypeEntity entity)
        {
            return new SitemapPageSettings
            {
                ContentTypeId = entity.ContentTypeId,
                ContentTypeGuid = entity.ContentTypeGuid,
                HideFromSitemap = entity.HideFromSitemap,
                ChangeFrequency = entity.ChangeFrequency,
                Priority = entity.Priority
            };
        }
    }
}
