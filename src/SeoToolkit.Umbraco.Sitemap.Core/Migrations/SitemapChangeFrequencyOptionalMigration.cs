using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapChangeFrequencyOptionalMigration : MigrationBase
    {
        public SitemapChangeFrequencyOptionalMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (ColumnExists("SeoToolkitSitemapPageType", "ChangeFrequency"))
            {
                Alter.Table("SeoToolkitSitemapPageType").AlterColumn("ChangeFrequency").AsString().Nullable().Do();
            }
        }
    }
}