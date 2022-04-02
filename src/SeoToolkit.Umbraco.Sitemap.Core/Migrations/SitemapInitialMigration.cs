using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapInitialMigration : MigrationBase
    {
        public SitemapInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("uSeoToolkitSitemapPageType"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSitemapPageType', 'SeoToolkitSitemapPageType'");
            }
            else if (!TableExists("SeoToolkitSitemapPageType"))
            {
                Create.Table<SitemapPageTypeEntity>().Do();
            }
        }
    }
}
