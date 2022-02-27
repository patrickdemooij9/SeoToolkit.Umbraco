using Umbraco.Cms.Core.Packaging;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsMigrationPlan : PackageMigrationPlan
    {
        public MetaFieldsMigrationPlan()
            : base("SEO Toolkit: Meta Fields", "uSeoToolkit_MetaFields_Migration")
        { }

        protected override void DefinePlan()
        {
            To<MetaFieldsInitialMigration>("state-1");
        }
    }
}
