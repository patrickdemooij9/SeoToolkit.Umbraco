using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsInitialMigration : AsyncMigrationBase
    {
        public MetaFieldsInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
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
            return Task.CompletedTask;
        }
    }
}
