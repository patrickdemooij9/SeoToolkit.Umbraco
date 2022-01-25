using System;
using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.Common.Core.Models.Database;

namespace uSeoToolkit.Umbraco.Common.Core.Migrations
{
    public class SeoSettingsInitialMigration : MigrationBase
    {
        public SeoSettingsInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("uSeoToolkitSeoSettings"))
            {
                Create.Table<SeoSettingsEntity>().Do();
            }
        }
    }
}
