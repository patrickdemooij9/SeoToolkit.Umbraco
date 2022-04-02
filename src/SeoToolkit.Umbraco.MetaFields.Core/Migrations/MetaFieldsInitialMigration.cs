using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsInitialMigration : MigrationBase
    {
        public MetaFieldsInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("uSeoToolkitMetaFieldsSettings"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitMetaFieldsSettings', 'SeoToolkitMetaFieldsSettings'");
            }
            else if (!TableExists("SeoToolkitMetaFieldsSettings"))
            {
                Create.Table<MetaFieldsSettingsEntity>().Do();
            }

            if (TableExists("uSeoToolkitMetaFieldsValue"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitMetaFieldsValue', 'SeoToolkitMetaFieldsValue'");
            }
            else if (!TableExists("SeoToolkitMetaFieldsValue"))
            {
                Create.Table<MetaFieldsValueEntity>().Do();
            }
        }
    }
}
