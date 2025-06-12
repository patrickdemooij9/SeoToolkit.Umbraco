using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    internal class OldUrlIndexMigration : MigrationBase
    {
        public OldUrlIndexMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!IndexExists("IX_SeoToolkitOldUrl"))
            {
                CreateIndex<RedirectEntity>("IX_SeoToolkitOldUrl");
            }
        }
    }
}
