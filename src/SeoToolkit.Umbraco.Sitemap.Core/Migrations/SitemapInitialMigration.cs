using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapInitialMigration : AsyncMigrationBase
    {
        public SitemapInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (TableExists("uSeoToolkitSitemapPageType"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSitemapPageType', 'SeoToolkitSitemapPageType'");
            }
            else if (!TableExists("SeoToolkitSitemapPageType"))
            {
                Create.Table<SitemapPageTypeEntity>().Do();
            }
            return Task.CompletedTask;
        }
    }
}
