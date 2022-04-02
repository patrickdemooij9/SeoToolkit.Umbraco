using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.Common.Core.Models.Database;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class SeoSettingsInitialMigration : MigrationBase
    {
        public SeoSettingsInitialMigration(IMigrationContext context)
            : base(context)
        { }

        protected override void Migrate()
        {
            if (TableExists("uSeoToolkitSeoSettings"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSeoSettings', 'SeoToolkitSeoSettings'");
            }
            else if (!TableExists("SeoToolkitSeoSettings"))
            {
                Create.Table<SeoSettingsEntity>().Do();
            }
        }
    }
}
