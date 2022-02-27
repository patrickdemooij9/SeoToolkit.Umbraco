using Umbraco.Cms.Core.Packaging;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    public class SiteAuditMigrationPlan : PackageMigrationPlan
    {
        public SiteAuditMigrationPlan()
            : base("SEO Toolkit: Site Audit", "uSeoToolkit_SiteAudit_Migration")
        { }

        protected override void DefinePlan()
        {
            To<SiteAuditInitialMigration>("state-1");
        }
    }
}
