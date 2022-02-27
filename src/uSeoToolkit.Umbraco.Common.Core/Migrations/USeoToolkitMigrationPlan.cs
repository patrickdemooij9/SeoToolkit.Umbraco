using Umbraco.Cms.Core.Packaging;

namespace uSeoToolkit.Umbraco.Common.Core.Migrations
{
    public class USeoToolkitMigrationPlan : PackageMigrationPlan
    {
        public USeoToolkitMigrationPlan()
            : base("SEO Toolkit", "uSeoToolkit_Common_Migration")
        { }

        protected override void DefinePlan()
        {
            To<SeoSettingsInitialMigration>("state-1");
            To<AddSeoToolkitSectionToAdminUserGroupMigration>("state-2");
            To<CreateSeoToolkitUserGroupMigration>("state-3");
        }
    }
}
