using Umbraco.Cms.Infrastructure.Migrations;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Database;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    public class SiteAuditInitialMigration : MigrationBase
    {
        public SiteAuditInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (!TableExists("uSeoToolkitSiteAudit"))
            {
                Create.Table<SiteAuditEntity>().Do();
            }
            if (!TableExists("uSeoToolkitSiteCheck"))
            {
                Create.Table<SiteCheckEntity>().Do();
            }
            if (!TableExists("uSeoToolkitSiteAuditCheck"))
            {
                Create.Table<SiteAuditCheckEntity>().Do();
            }
            if (!TableExists("uSeoToolkitSiteAuditPage"))
            {
                Create.Table<SiteAuditPageEntity>().Do();
            }
            if (!TableExists("uSeoToolkitSiteAuditCheckResult"))
            {
                Create.Table<SiteAuditCheckResultEntity>().Do();
            }
        }
    }
}
