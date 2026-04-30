using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class InitialRedirectsMigration : AsyncMigrationBase
    {
        public InitialRedirectsMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (TableExists("uSeoToolkitRedirects"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitRedirects', 'SeoToolkitRedirects'");
            }
            else if (!TableExists("SeoToolkitRedirects"))
            {
                Create.Table<RedirectEntity>().Do();
            }
            return Task.CompletedTask;
        }
    }
}
