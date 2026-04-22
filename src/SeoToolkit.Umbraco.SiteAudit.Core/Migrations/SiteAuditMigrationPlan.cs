using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    public class SiteAuditMigrationPlan : PackageMigrationPlan
    {
        public SiteAuditMigrationPlan()
            : base("SEO Toolkit: Site Audit", "SeoToolkit_SiteAudit_Migration")
        { }

        public override bool IgnoreCurrentState => false;

        protected override void DefinePlan()
        {
            To<SiteAuditInitialMigration>("state-1");
            To<SiteAuditExternalSourceMigration>("state-2");
        }
    }
}
