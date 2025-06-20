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
                Create.Index("IX_SeoToolkitOldUrl").OnTable("SeoToolkitRedirects")
                    .WithOptions().NonClustered()
                    .OnColumn(nameof(RedirectEntity.OldUrl)).Ascending()
                    .OnColumn(nameof(RedirectEntity.IsEnabled)).Ascending()
                    .Do();
            }
            if (!IndexExists("IX_SeoToolkitRegex"))
            {
                Create.Index("IX_SeoToolkitRegex").OnTable("SeoToolkitRedirects")
                    .WithOptions().NonClustered()
                    .OnColumn(nameof(RedirectEntity.IsRegex)).Ascending()
                    .Do();
            }
        }
    }
}
