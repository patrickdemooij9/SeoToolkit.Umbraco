using NPoco;
using SeoToolkit.Umbraco.Common.Core.Migrations;
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
                MigrationHelper.RecreateTable<SitemapPageTypeEntity>(Database, Create, Sql(), "SeoToolkitSitemapPageType");
            }
            else
            {
                Alter.Table("SeoToolkitSitemapPageType").AlterColumn("ChangeFrequency").AsString().Nullable().Do();
            }
        }
    }
}