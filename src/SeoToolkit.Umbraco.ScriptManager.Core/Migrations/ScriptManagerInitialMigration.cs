using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Database;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerInitialMigration : AsyncMigrationBase
    {
        public ScriptManagerInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (TableExists("uSeoToolkitScript"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitScript', 'SeoToolkitScript'");
            }
            else if (!TableExists("SeoToolkitScript"))
                Create.Table<ScriptEntity>().Do();

            return Task.CompletedTask;
        }
    }
}
