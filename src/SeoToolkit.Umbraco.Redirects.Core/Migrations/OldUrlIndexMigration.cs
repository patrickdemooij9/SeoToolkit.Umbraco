using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    internal class OldUrlIndexMigration : AsyncMigrationBase
    {
        public OldUrlIndexMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!IndexExists("IX_SeoToolkitOldUrl"))
            {
                CreateIndex<RedirectEntity>("IX_SeoToolkitOldUrl");
            }
            if (!IndexExists("IX_SeoToolkitRegex"))
            {
                CreateIndex<RedirectEntity>("IX_SeoToolkitRegex");
            }
            return Task.CompletedTask;
        }
    }
}
