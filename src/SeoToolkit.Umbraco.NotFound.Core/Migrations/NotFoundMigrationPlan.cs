using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.NotFound.Core.Migrations
{
    public class NotFoundMigrationPlan : PackageMigrationPlan
    {
        public NotFoundMigrationPlan() : base("SEO Toolkit: NotFound", "SeoToolkit_NotFound_Migration")
        {
        }

        public override bool IgnoreCurrentState => false;

        protected override void DefinePlan()
        {
            To<NotFoundUmbraco13Migration>("state-1");
            To<NotFoundDomainMigration>("state-2");
        }
    }
}
