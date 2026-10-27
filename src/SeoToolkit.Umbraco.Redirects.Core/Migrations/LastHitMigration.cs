using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    internal class LastHitMigration : AsyncMigrationBase
    {
        public LastHitMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!ColumnExists("SeoToolkitRedirects", "LastHit"))
            {
                AddColumn<RedirectEntity>("LastHit");
            }
            return Task.CompletedTask;
        }
    }
}
