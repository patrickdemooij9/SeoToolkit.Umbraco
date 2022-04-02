using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class SeoToolkitMigrationPlan : PackageMigrationPlan
    {
        public SeoToolkitMigrationPlan()
            : base("SEO Toolkit", "SeoToolkit_Common_Migration")
        { }

        protected override void DefinePlan()
        {
            To<SeoSettingsInitialMigration>("state-1");
            To<AddSeoToolkitSectionToAdminUserGroupMigration>("state-2");
            To<CreateSeoToolkitUserGroupMigration>("state-3");
        }
    }
}
