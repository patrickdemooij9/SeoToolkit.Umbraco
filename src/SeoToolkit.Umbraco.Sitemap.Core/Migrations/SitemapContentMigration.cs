using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapContentMigration : MigrationBase
    {
        public SitemapContentMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("SeoToolkitSitemapContent"))
            {
                Create.Table<SitemapContentEntity>().Do();
            }
        }
    }
}
