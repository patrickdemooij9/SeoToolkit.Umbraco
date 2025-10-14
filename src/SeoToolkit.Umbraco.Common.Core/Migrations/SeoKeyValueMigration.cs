using SeoToolkit.Umbraco.Common.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class SeoKeyValueMigration : AsyncMigrationBase
    {
        public SeoKeyValueMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!TableExists("SeoToolkitSeoKeyValues"))
            {
                Create.Table<SeoKeyValueEntity>().Do();
            }
            return Task.CompletedTask;
        }
    }
}
