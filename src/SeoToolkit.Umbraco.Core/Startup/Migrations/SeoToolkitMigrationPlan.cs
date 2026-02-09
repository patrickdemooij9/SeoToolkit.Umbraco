using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.Core.Startup.Migrations
{
    internal class SeoToolkitMigrationPlan : PackageMigrationPlan
    {
        public SeoToolkitMigrationPlan() : base("SEO Toolkit", "SeoToolkit_Migration")
        {
        }

        protected override void DefinePlan()
        {
            To<SitemapInRobotsTxtMigration>("state-1");
        }
    }
}
