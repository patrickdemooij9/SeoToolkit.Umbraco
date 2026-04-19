using System;
using System.Linq;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.Sitemap.Core.Repositories
{
    public class SitemapContentRepository : ISitemapContentRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SitemapContentRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public SitemapContentSettings? Get(Guid nodeKey)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<SitemapContentEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SitemapContentEntity>()
                .Where<SitemapContentEntity>(it => it.NodeKey == nodeKey));

            return entity is null ? null : MapFromEntity(entity);
        }

        public void Set(SitemapContentSettings settings)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            scope.Database.Save(new SitemapContentEntity
            {
                NodeKey = settings.NodeKey,
                ExcludeFromSitemap = settings.ExcludeFromSitemap,
                ChangeFrequency = settings.ChangeFrequency,
                Priority = settings.Priority
            });
        }

        public SitemapContentSettings[] GetAll()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return scope.Database
                .Fetch<SitemapContentEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SitemapContentEntity>())
                .Select(MapFromEntity)
                .ToArray();
        }

        private static SitemapContentSettings MapFromEntity(SitemapContentEntity entity)
        {
            return new SitemapContentSettings
            {
                NodeKey = entity.NodeKey,
                ExcludeFromSitemap = entity.ExcludeFromSitemap,
                ChangeFrequency = entity.ChangeFrequency,
                Priority = entity.Priority
            };
        }
    }
}
