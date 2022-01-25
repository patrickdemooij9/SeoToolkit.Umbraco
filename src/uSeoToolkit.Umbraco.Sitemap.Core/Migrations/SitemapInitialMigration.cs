using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.Sitemap.Core.Models.Database;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapInitialMigration : MigrationBase
    {
        public SitemapInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("uSeoToolkitSitemapPageType"))
            {
                Create.Table<SitemapPageTypeEntity>().Do();
            }
        }
    }
}
