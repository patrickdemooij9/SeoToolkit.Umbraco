using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Migrations
{
    public class RobotsTxtMigrationPlan : PackageMigrationPlan
    {
        public RobotsTxtMigrationPlan()
            : base("SEO Toolkit: Robots.txt", "SeoToolkit_RobotsTxt_Migration")
        { }

        protected override void DefinePlan()
        {
            To<InitialRobotsTxtMigration>("state-1");
            To<RobotsTxtDomainMigration>("state-2");
            To<RobotsTxtGuidIdMigration>("state-3");
        }
    }
}
