using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapMigrationPlan : PackageMigrationPlan
    {
        public SitemapMigrationPlan()
        : base("SEO Toolkit: Sitemap", "SeoToolkit_Sitemap_Migration")
        { }

        protected override void DefinePlan()
        {
            To<SitemapInitialMigration>("state-1");
        }
    }
}
