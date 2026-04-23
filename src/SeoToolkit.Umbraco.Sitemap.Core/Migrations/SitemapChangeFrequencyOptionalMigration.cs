using NPoco;
using SeoToolkit.Umbraco.Common.Core.Migrations;
using SeoToolkit.Umbraco.Sitemap.Core.Migrations.Entities._5_0_0;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapChangeFrequencyOptionalMigration : AsyncMigrationBase
    {
        public SitemapChangeFrequencyOptionalMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!ColumnExists("SeoToolkitSitemapPageType", "ChangeFrequency")) return Task.CompletedTask;

            if (DatabaseType == DatabaseType.SQLite)
            {
                //SQLite doesn't support normal altering of columns. https://github.com/umbraco/Umbraco-CMS/issues/12676
                MigrationHelper.RecreateTable<SitemapPageTypeEntity_5>(Database, Create, Sql(), "SeoToolkitSitemapPageType");
            }
            else
            {
                Alter.Table("SeoToolkitSitemapPageType").AlterColumn("ChangeFrequency").AsString().Nullable().Do();
            }
            return Task.CompletedTask;
        }
    }
}