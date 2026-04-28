using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapContentMigrationPlan : PackageMigrationPlan
    {
        public SitemapContentMigrationPlan()
        : base("SEO Toolkit: Sitemap Content", "SeoToolkit_SitemapContent_Migration")
        { }

        public override bool IgnoreCurrentState => false;

        protected override void DefinePlan()
        {
            To<SitemapContentMigration>("state-1");
            To<SitemapContentExcludeMigration>("state-2");
        }
    }
}
