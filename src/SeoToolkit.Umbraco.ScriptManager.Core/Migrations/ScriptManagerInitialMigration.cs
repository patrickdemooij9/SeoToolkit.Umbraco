using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Database;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerInitialMigration : MigrationBase
    {
        public ScriptManagerInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("uSeoToolkitScript"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitScript', 'SeoToolkitScript'");
            }
            else if (!TableExists("SeoToolkitScript"))
                Create.Table<ScriptEntity>().Do();
        }
    }
}
