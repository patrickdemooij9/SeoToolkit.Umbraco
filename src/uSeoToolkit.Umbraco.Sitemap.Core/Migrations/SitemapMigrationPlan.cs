using Umbraco.Cms.Core.Packaging;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapMigrationPlan : PackageMigrationPlan
    {
        public SitemapMigrationPlan()
        : base("SEO Toolkit: Sitemap", "uSeoToolkit_Sitemap_Migration")
        { }

        protected override void DefinePlan()
        {
            To<SitemapInitialMigration>("state-1");
        }
    }
}
