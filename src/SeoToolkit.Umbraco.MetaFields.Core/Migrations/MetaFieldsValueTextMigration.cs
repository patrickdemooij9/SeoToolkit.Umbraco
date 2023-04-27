using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsValueTextMigration : MigrationBase
    {
        public MetaFieldsValueTextMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            Database.Execute($"ALTER TABLE SeoToolkitMetaFieldsValue ALTER COLUMN UserValue nvarchar(max);");
        }
    }
}
