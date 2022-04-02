using System;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;

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
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                scope.Database.Save(new SitemapPageTypeEntity
                {
                    ContentTypeId = settings.ContentTypeId,
                    HideFromSitemap = settings.HideFromSitemap,
                    ChangeFrequency = settings.ChangeFrequency,
                    Priority = settings.Priority
                });
            }
        }

        public SitemapPageSettings Get(int contentTypeId)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var entity = scope.Database.FirstOrDefault<SitemapPageTypeEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SitemapPageTypeEntity>()
                    .Where<SitemapPageTypeEntity>(it => it.ContentTypeId == contentTypeId));

                return entity is null
                    ? null
                    : new SitemapPageSettings
                    {
                        ContentTypeId = entity.ContentTypeId,
                        HideFromSitemap = entity.HideFromSitemap,
                        ChangeFrequency = entity.ChangeFrequency,
                        Priority = entity.Priority
                    };
            }
        }
    }
}
