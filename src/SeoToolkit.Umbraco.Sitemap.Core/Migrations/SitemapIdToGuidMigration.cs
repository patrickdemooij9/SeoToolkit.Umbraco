using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.Sitemap.Core.Migrations.Entities._5_0_0;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapIdToGuidMigration : AsyncMigrationBase
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IKeyValueService _keyValueService;

        public SitemapIdToGuidMigration(IMigrationContext context, IContentTypeService contentTypeService, IKeyValueService keyValueService) : base(context)
        {
            _contentTypeService = contentTypeService;
            _keyValueService = keyValueService;
        }

        protected override Task MigrateAsync()
        {
            // We have a dependency on the common migration to have run first, otherwise the cleanup will be a big mess
            MigrationHelper.EnsureMigration("SeoToolkit_Common_Migration", 6, _keyValueService);

            if (ColumnExists("SeoToolkitSitemapPageType", "ContentTypeGuid")) return Task.CompletedTask;

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ADD ContentTypeGuid UNIQUEIDENTIFIER NULL");
            foreach (var entry in Database.Fetch<SitemapPageTypeEntity_5>(Sql().SelectAll().From<SitemapPageTypeEntity_5>()))
            {
                var contentType = _contentTypeService.Get(entry.ContentTypeId);
                if (contentType is null) {
                    continue;
                };

                Database.Update(Sql().Update<SitemapPageTypeEntity>((it) => it.Set(c => c.ContentTypeGuid, contentType.Key)).Where<SitemapPageTypeEntity>(it => it.ContentTypeId == entry.ContentTypeId));
            }

            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                MigrationHelper.RecreateTable<SitemapPageTypeEntity>(Database, Create, Sql(), "SeoToolkitSitemapPageType");
                return Task.CompletedTask;
            }

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ALTER COLUMN ContentTypeGuid UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType DROP CONSTRAINT pk_SeoToolkitSitemapPageType");
            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ADD CONSTRAINT pk_SeoToolkitSitemapPageType PRIMARY KEY (ContentTypeGuid)");
            return Task.CompletedTask;
        }
    }
}