using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapIdToGuidMigration : AsyncMigrationBase
    {
        public SitemapIdToGuidMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (ColumnExists("SeoToolkitSitemapPageType", "ContentTypeGuid")) return Task.CompletedTask;

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ADD COLUMN ContentTypeGuid UNIQUEIDENTIFIER NULL");
            Database.Execute("UPDATE SeoToolkitSitemapPageType SET ContentTypeGuid = NEWID()"); // TODO: This is wrong, infer from ContentTypeId
            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ALTER COLUMN ContentTypeGuid UNIQUEIDENTIFIER NOT NULL");

            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType DROP CONSTRAINT pk_SeoToolkitSitemapPageType");
            Database.Execute("ALTER TABLE SeoToolkitSitemapPageType ADD CONSTRAINT pk_SeoToolkitSitemapPageType PRIMARY KEY (ContentTypeGuid)");
            return Task.CompletedTask;
        }
    }
}