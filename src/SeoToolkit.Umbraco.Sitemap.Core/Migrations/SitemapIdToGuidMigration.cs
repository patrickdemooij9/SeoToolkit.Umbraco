using System.Threading.Tasks;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapIdToGuidMigration : AsyncMigrationBase
    {
        private readonly IContentTypeService _contentTypeService;

        public SitemapIdToGuidMigration(IMigrationContext context, IContentTypeService contentTypeService) : base(context)
        {
            _contentTypeService = contentTypeService;
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitSitemapPageType", "ContentTypeGuid")) return Task.CompletedTask;

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ADD COLUMN ContentTypeGuid UNIQUEIDENTIFIER NULL");
            foreach (var entry in Database.Fetch<SitemapPageTypeEntity>(Sql().SelectAll().From<SitemapPageTypeEntity>()))
            {
                var contentType = _contentTypeService.Get(entry.ContentTypeId);
                if (contentType is null) continue;

                Database.Execute("UPDATE SeoToolkitSitemapPageType SET ContentTypeGuid = @0 WHERE ContentTypeId = @1",
                    contentType.Key, entry.ContentTypeId);
            }

            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                Database.Execute("ALTER TABLE SeoToolkitSitemapPageType RENAME TO old_SeoToolkitSitemapPageType;");
                Create.Table<SitemapPageTypeEntity>().Do();
                Database.InsertBulk(Database.Fetch<SitemapPageTypeEntity>(Sql()
                    .SelectAll()
                    .From("old_SeoToolkitSitemapPageType")));
                return Task.CompletedTask;
            }

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ALTER COLUMN ContentTypeGuid UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType DROP CONSTRAINT pk_SeoToolkitSitemapPageType");
            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ADD CONSTRAINT pk_SeoToolkitSitemapPageType PRIMARY KEY (ContentTypeGuid)");
            return Task.CompletedTask;
        }
    }
}