using SeoToolkit.Umbraco.Common.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class SeoToolkitDomainBaseUrlMigration : AsyncMigrationBase
    {
        public SeoToolkitDomainBaseUrlMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (TableExists("SeoToolkitDomainCollections") && !ColumnExists("SeoToolkitDomainCollections", "BaseUrl"))
            {
                if (DatabaseType == NPoco.DatabaseType.SQLite)
                {
                    MigrationHelper.RecreateTable<SeoDomainCollectionEntity>(Database, Create, Sql(), "SeoToolkitDomainCollections");
                    return Task.CompletedTask;
                }

                Alter.Table("SeoToolkitDomainCollections").AddColumn("BaseUrl").AsString(500).Nullable().Do();
            }
            return Task.CompletedTask;
        }
    }
}
