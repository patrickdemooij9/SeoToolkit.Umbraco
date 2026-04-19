using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations
{
    public class SitemapContentExcludeMigration : MigrationBase
    {
        public SitemapContentExcludeMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!ColumnExists("SeoToolkitSitemapContent", "ExcludeFromSitemap"))
            {
                Alter.Table("SeoToolkitSitemapContent")
                    .AddColumn("ExcludeFromSitemap")
                    .AsBoolean()
                    .NotNullable()
                    .WithDefaultValue(false)
                    .Do();
            }
        }
    }
}
