using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class SchemaEntryDisplayNameMigration : MigrationBase
    {
        public SchemaEntryDisplayNameMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("SeoToolkitSchemaEntry") && !ColumnExists("SeoToolkitSchemaEntry", "DisplayName"))
            {
                Alter.Table("SeoToolkitSchemaEntry")
                    .AddColumn("DisplayName").AsString(500).Nullable()
                    .Do();
            }
        }
    }
}
