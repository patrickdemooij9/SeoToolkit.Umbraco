using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Database;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Migrations
{
    public class InitialRobotsTxtMigration : MigrationBase
    {
        public InitialRobotsTxtMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("uSeoToolkitRobotsTxt"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitRobotsTxt', 'SeoToolkitRobotsTxt'");
            }
            else if (!TableExists("SeoToolkitRobotsTxt"))
            {
                Create.Table<RobotsTxtEntity>().Do();
            }
        }
    }
}
