using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class InitialRedirectsMigration : MigrationBase
    {
        public InitialRedirectsMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("uSeoToolkitRedirects"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitRedirects', 'SeoToolkitRedirects'");
            }
            else if (!TableExists("SeoToolkitRedirects"))
            {
                Create.Table<RedirectEntity>().Do();
            }
        }
    }
}
