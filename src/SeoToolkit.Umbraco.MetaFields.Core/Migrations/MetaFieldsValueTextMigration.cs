using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsValueTextMigration : MigrationBase
    {
        public MetaFieldsValueTextMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                //SQLite doesn't support normal altering of columns. https://github.com/umbraco/Umbraco-CMS/issues/12676
                Database.Execute("ALTER TABLE SeoToolkitMetaFieldsValue RENAME TO old_SeoToolkitMetaFieldsValue;");
                Create.Table<MetaFieldsValueEntity>().Do();
                Database.InsertBulk(Database.Fetch<MetaFieldsValueEntity>(Sql()
                    .SelectAll()
                    .From("old_SeoToolkitMetaFieldsValue")));
            }
            else
            {
                Database.Execute($"ALTER TABLE SeoToolkitMetaFieldsValue ALTER COLUMN UserValue nvarchar(max);");
            }
        }
    }
}
