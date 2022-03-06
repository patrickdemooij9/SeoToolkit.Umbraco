using Umbraco.Cms.Core.Packaging;

namespace uSeoToolkit.Umbraco.Redirects.Core.Migrations
{
    public class RedirectsMigrationPlan : PackageMigrationPlan
    {
        public RedirectsMigrationPlan()
            : base("SEO Toolkit: Redirects", "uSeoToolkit_Redirects_Migration")
        { }

        protected override void DefinePlan()
        {
            To<InitialRedirectsMigration>("state-1");
        }
    }
}
