using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Database;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    public class SiteAuditInitialMigration : MigrationBase
    {
        public SiteAuditInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            if (TableExists("uSeoToolkitSiteAudit"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSiteAudit', 'SeoToolkitSiteAudit'");
            }
            else if (!TableExists("SeoToolkitSiteAudit"))
            {
                Create.Table<SiteAuditEntity>().Do();
            }

            if (TableExists("uSeoToolkitSiteCheck"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSiteCheck', 'SeoToolkitSiteCheck'");
            }
            else if (!TableExists("SeoToolkitSiteCheck"))
            {
                Create.Table<SiteCheckEntity>().Do();
            }

            if (TableExists("uSeoToolkitSiteAuditCheck"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSiteAuditCheck', 'SeoToolkitSiteAuditCheck'");
            }
            else if (!TableExists("SeoToolkitSiteAuditCheck"))
            {
                Create.Table<SiteAuditCheckEntity>().Do();
            }

            if (TableExists("uSeoToolkitSiteAuditPage"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSiteAuditPage', 'SeoToolkitSiteAuditPage'");
            }
            else if (!TableExists("SeoToolkitSiteAuditPage"))
            {
                Create.Table<SiteAuditPageEntity>().Do();
            }

            if (TableExists("uSeoToolkitSiteAuditCheckResult"))
            {
                Database.Execute("exec sp_rename 'uSeoToolkitSiteAuditCheckResult', 'SeoToolkitSiteAuditCheckResult'");
            }
            else if (!TableExists("SeoToolkitSiteAuditCheckResult"))
            {
                Create.Table<SiteAuditCheckResultEntity>().Do();
            }
        }
    }
}
