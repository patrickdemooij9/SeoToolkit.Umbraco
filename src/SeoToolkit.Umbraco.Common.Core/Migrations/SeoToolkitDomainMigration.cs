using SeoToolkit.Umbraco.Common.Core.Models.Database;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Common.Core.Migrations
{
    public class SeoToolkitDomainMigration : AsyncMigrationBase
    {
        public SeoToolkitDomainMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!TableExists("SeoToolkitDomainCollections"))
            {
                Create.Table<SeoDomainCollectionEntity>().Do();
            }
            if (!TableExists("SeoToolkitDomains"))
            {
                Create.Table<SeoDomainEntity>().Do();
            }
            if (!TableExists("SeoToolkitDomainSettings"))
            {
                Create.Table<SeoDomainSettingEntity>().Do();
            }
            return Task.CompletedTask;
        }
    }
}
