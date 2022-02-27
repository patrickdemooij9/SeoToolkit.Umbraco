using Umbraco.Cms.Core.Packaging;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Migrations
{
    public class ScriptManagerMigrationPlan : PackageMigrationPlan
    {
        public ScriptManagerMigrationPlan()
        : base("SEO Toolkit: Script Manager", "uSeoToolkit_ScriptManager_Migration")
        { }

        protected override void DefinePlan()
        {
            To<ScriptManagerInitialMigration>("state-1");
        }
    }
}
