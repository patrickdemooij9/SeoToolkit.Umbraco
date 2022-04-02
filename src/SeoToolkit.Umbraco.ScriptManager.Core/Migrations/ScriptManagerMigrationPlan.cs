using Umbraco.Cms.Core.Packaging;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerMigrationPlan : PackageMigrationPlan
    {
        public ScriptManagerMigrationPlan()
        : base("SEO Toolkit: Script Manager", "SeoToolkit_ScriptManager_Migration")
        { }

        protected override void DefinePlan()
        {
            To<ScriptManagerInitialMigration>("state-1");
        }
    }
}
