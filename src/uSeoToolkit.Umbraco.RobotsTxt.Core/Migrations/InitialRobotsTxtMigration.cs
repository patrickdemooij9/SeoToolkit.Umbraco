using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Models.Database;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Migrations
{
    public class InitialRobotsTxtMigration : MigrationBase
    {
        public InitialRobotsTxtMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("uSeoToolkitRobotsTxt"))
            {
                Create.Table<RobotsTxtEntity>().Do();
            }
        }
    }
}
