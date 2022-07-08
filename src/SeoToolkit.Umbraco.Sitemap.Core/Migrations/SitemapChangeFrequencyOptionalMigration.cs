using NPoco;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapChangeFrequencyOptionalMigration : MigrationBase
    {
        public SitemapChangeFrequencyOptionalMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("SeoToolkitSitemapPageType", "ChangeFrequency")) return;

            if (DatabaseType == DatabaseType.SQLite)
            {
                //SQLite doesn't support normal altering of columns. https://github.com/umbraco/Umbraco-CMS/issues/12676
                Database.Execute("ALTER TABLE SeoToolkitSitemapPageType RENAME TO old_SeoToolkitSitemapPageType;");
                Create.Table<SitemapPageTypeEntity>().Do();
                Database.InsertBulk(Database.Fetch<SitemapPageTypeEntity>(Sql()
                    .SelectAll()
                    .From("old_SeoToolkitSitemapPageType")));
            }
            else
            {
                Alter.Table("SeoToolkitSitemapPageType").AlterColumn("ChangeFrequency").AsString().Nullable().Do();
            }
        }
    }
}