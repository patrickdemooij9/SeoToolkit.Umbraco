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
                Alter.Table("SeoToolkitDomainCollections").AddColumn("BaseUrl").AsString(500).Nullable().Do();
            }
            return Task.CompletedTask;
        }
    }
}
