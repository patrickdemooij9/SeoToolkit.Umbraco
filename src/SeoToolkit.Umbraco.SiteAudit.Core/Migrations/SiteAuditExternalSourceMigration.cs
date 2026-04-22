using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    public class SiteAuditExternalSourceMigration : AsyncMigrationBase
    {
        public SiteAuditExternalSourceMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            if (!ColumnExists("SeoToolkitSiteAudit", "ExternalAuditId"))
            {
                Database.Execute("ALTER TABLE SeoToolkitSiteAudit ADD ExternalAuditId UNIQUEIDENTIFIER NULL");
            }

            return Task.CompletedTask;
        }
    }
}
