using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Database;

namespace uSeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class InitialRedirectsMigration : MigrationBase
    {
        public InitialRedirectsMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("uSeoToolkitRedirects"))
            {
                Create.Table<RedirectEntity>().Do();
            }
        }
    }
}
