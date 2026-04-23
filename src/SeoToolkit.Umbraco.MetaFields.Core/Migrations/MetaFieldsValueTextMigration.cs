using SeoToolkit.Umbraco.MetaFields.Core.Migrations.Entities._5_0_0;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations
{
    public class MetaFieldsValueTextMigration : AsyncMigrationBase
    {
        public MetaFieldsValueTextMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                //SQLite doesn't support normal altering of columns. https://github.com/umbraco/Umbraco-CMS/issues/12676
                Database.Execute("DROP TABLE IF EXISTS old_SeoToolkitMetaFieldsValue;");
                Database.Execute("ALTER TABLE SeoToolkitMetaFieldsValue RENAME TO old_SeoToolkitMetaFieldsValue;");
                Create.Table<MetaFieldsValueEntity_5>().Do();
                Database.InsertBulk(Database.Fetch<MetaFieldsValueEntity_5>(Sql()
                    .SelectAll()
                    .From("old_SeoToolkitMetaFieldsValue")));
            }
            else
            {
                Database.Execute($"ALTER TABLE SeoToolkitMetaFieldsValue ALTER COLUMN UserValue nvarchar(max);");
            }
            return Task.CompletedTask;
        }
    }
}
