using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Database;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerInitialMigration : MigrationBase
    {
        public ScriptManagerInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("uSeoToolkitScript"))
                Create.Table<ScriptEntity>().Do();
        }
    }
}
