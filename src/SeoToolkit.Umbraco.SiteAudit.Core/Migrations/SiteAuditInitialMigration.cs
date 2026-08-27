using System;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Database;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    public class SiteAuditInitialMigration : AsyncMigrationBase
    {
        public SiteAuditInitialMigration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            EnsureTable<SiteAuditEntity>("uSeoToolkitSiteAudit", "SeoToolkitSiteAudit");
            EnsureTable<SiteCheckEntity>("uSeoToolkitSiteCheck", "SeoToolkitSiteCheck");
            EnsureTable<SiteAuditCheckEntity>("uSeoToolkitSiteAuditCheck", "SeoToolkitSiteAuditCheck");
            EnsureTable<SiteAuditPageEntity>("uSeoToolkitSiteAuditPage", "SeoToolkitSiteAuditPage");
            EnsureTable<SiteAuditCheckResultEntity>("uSeoToolkitSiteAuditCheckResult", "SeoToolkitSiteAuditCheckResult");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Renames the pre-6.0 table if it is still present, otherwise creates the table from scratch.
        /// </summary>
        private void EnsureTable<TEntity>(string legacyTableName, string tableName)
        {
            if (TableExists(legacyTableName))
            {
                RenameTable(legacyTableName, tableName);
            }
            else if (!TableExists(tableName))
            {
                Create.Table<TEntity>().Do();
            }
        }

        private void RenameTable(string from, string to)
        {
            //sp_rename is SQL Server only - SQLite (and the ANSI standard) use ALTER TABLE ... RENAME TO.
            if (DatabaseType == NPoco.DatabaseType.SQLite)
            {
                Database.Execute($"ALTER TABLE \"{from}\" RENAME TO \"{to}\"");
                return;
            }

            Database.Execute($"exec sp_rename '{from}', '{to}'");
        }
    }
}
