using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    [Weight(10)]
    public class SeoToolkitMigrationPlan : PackageMigrationPlan
    {
        public SeoToolkitMigrationPlan()
            : base("SEO Toolkit", "SeoToolkit_Common_Migration")
        { }

        public override bool IgnoreCurrentState => false;

        protected override void DefinePlan()
        {
            To<SeoSettingsInitialMigration>("state-1");
            To<AddSeoToolkitSectionToAdminUserGroupMigration>("state-2");
            To<CreateSeoToolkitUserGroupMigration>("state-3");
            To<SeoToolkitDomainMigration>("state-4");
            To<SeoKeyValueMigration>("state-5");
            To<CommonIdToGuidMigration>("state-6");
            To<SeoToolkitDomainBaseUrlMigration>("state-7");
        }
    }
}
