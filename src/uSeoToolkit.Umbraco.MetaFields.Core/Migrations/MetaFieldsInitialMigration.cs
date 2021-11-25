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
            if (!TableExists("uSeoToolkitDocumentTypeSettings"))
            {
                Create.Table<DocumentTypeSettingsEntity>().Do();
            }

            if (!TableExists("uSeoToolkitSeoValue"))
            {
                Create.Table<SeoValueEntity>().Do();
            }
        }
    }
}
