using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Database;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Migrations
{
    public class InitialRobotsTxtMigration : AsyncMigrationBase
    {
        public InitialRobotsTxtMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (TableExists("uSeoToolkitRobotsTxt"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitRobotsTxt', 'SeoToolkitRobotsTxt'");
            }
            else if (!TableExists("SeoToolkitRobotsTxt"))
            {
                Create.Table<RobotsTxtEntity>().Do();
            }
            return Task.CompletedTask;
        }
    }
}
