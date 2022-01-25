using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Database;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsInitialMigration : MigrationBase
    {
        public MetaFieldsInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("uSeoToolkitMetaFieldsSettings"))
            {
                Create.Table<MetaFieldsSettingsEntity>().Do();
            }

            if (!TableExists("uSeoToolkitMetaFieldsValue"))
            {
                Create.Table<MetaFieldsValueEntity>().Do();
            }
        }
    }
}
