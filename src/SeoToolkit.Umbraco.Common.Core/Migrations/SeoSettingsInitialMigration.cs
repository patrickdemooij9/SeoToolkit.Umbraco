using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.Common.Core.Models.Database;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class SeoSettingsInitialMigration : AsyncMigrationBase
    {
        public SeoSettingsInitialMigration(IMigrationContext context)
            : base(context)
        { }

        protected override Task MigrateAsync()
        {
            if (TableExists("uSeoToolkitSeoSettings"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSeoSettings', 'SeoToolkitSeoSettings'");
            }
            else if (!TableExists("SeoToolkitSeoSettings"))
            {
                Create.Table<SeoSettingsEntity>().Do();
            }
            return Task.CompletedTask;
        }
    }
}
