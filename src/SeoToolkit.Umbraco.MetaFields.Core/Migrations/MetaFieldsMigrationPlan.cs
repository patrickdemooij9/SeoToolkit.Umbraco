using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsMigrationPlan : PackageMigrationPlan
    {
        public MetaFieldsMigrationPlan()
            : base("SEO Toolkit: Meta Fields", "SeoToolkit_MetaFields_Migration")
        { }

        protected override void DefinePlan()
        {
            To<MetaFieldsInitialMigration>("state-1");
            To<MetaFieldsValueTextMigration>("state-2");
            To<MetaFieldsUmbraco14Migration>("state-3");
            To<MetaFieldsGuidIdMigration>("state-4");
        }
    }
}
