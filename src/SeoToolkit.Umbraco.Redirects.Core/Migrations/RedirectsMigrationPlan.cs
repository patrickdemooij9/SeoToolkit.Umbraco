using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class RedirectsMigrationPlan : PackageMigrationPlan
    {
        public RedirectsMigrationPlan()
            : base("SEO Toolkit: Redirects", "SeoToolkit_Redirects_Migration")
        { }

        public override bool IgnoreCurrentState => false;

        protected override void DefinePlan()
        {
            To<InitialRedirectsMigration>("state-1");
            To<CreatedByMigration>("state-2");
            To<IsEnabledMigration>("state-3");
            To<OldUrlIndexMigration>("state-4");
            To<RedirectGuidIdMigration>("state-5");
            To<RedirectCreatedByGuidMigration>("state-6");
        }
    }
}
