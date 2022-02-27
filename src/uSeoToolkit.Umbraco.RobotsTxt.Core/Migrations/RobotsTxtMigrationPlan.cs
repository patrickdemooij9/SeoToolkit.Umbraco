using Umbraco.Cms.Core.Packaging;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Migrations
{
    public class RobotsTxtMigrationPlan : PackageMigrationPlan
    {
        public RobotsTxtMigrationPlan()
            : base("SEO Toolkit: Robots.txt", "uSeoToolkit_RobotsTxt_Migration")
        { }

        protected override void DefinePlan()
        {
            To<InitialRobotsTxtMigration>("state-1");
        }
    }
}
