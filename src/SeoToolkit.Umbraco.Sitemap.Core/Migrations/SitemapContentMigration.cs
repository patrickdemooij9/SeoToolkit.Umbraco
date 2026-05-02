using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapContentMigration : AsyncMigrationBase
    {
        public SitemapContentMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!TableExists("SeoToolkitSitemapContent"))
            {
                Create.Table<SitemapContentEntity>().Do();
            }
            return Task.CompletedTask;
        }
    }
}
