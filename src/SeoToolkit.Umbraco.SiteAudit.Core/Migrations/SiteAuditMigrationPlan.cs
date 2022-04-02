using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    public class SiteAuditMigrationPlan : PackageMigrationPlan
    {
        public SiteAuditMigrationPlan()
            : base("SEO Toolkit: Site Audit", "SeoToolkit_SiteAudit_Migration")
        { }

        protected override void DefinePlan()
        {
            To<SiteAuditInitialMigration>("state-1");
        }
    }
}
