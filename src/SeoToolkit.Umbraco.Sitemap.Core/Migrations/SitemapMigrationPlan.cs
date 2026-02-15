using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapMigrationPlan : PackageMigrationPlan
    {
        public SitemapMigrationPlan()
        : base("SEO Toolkit: Sitemap", "SeoToolkit_Sitemap_Migration")
        { }

        public override bool IgnoreCurrentState => false;

        protected override void DefinePlan()
        {
            To<SitemapInitialMigration>("state-1");
            To<SitemapChangeFrequencyOptionalMigration>("state-2");
            To<SitemapIdToGuidMigration>("state-3");
        }
    }
}