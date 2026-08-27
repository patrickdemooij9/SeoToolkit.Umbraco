using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Database;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Migrations
{
    /// <summary>
    /// Adds the tables the reworked crawler writes to.
    /// <para>
    /// Runs from the previous schema are deliberately not carried across. A crawl is a snapshot
    /// of a site at one moment, so old results have very little value once the site has moved on,
    /// and translating them would mean inventing data the old schema never recorded - depth,
    /// redirect counts, indexability, per-check totals. The previous tables are left in place for
    /// now so nothing is destroyed by upgrading; a later migration removes them.
    /// </para>
    /// </summary>
    public class SiteAuditResultsV2Migration : AsyncMigrationBase
    {
        public SiteAuditResultsV2Migration(IMigrationContext context) : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            EnsureTable<SiteAuditRunEntity>(SiteAuditRunEntity.TableName);
            EnsureTable<SiteAuditResourceEntity>(SiteAuditResourceEntity.TableName);
            EnsureTable<SiteAuditIssueEntity>(SiteAuditIssueEntity.TableName);
            EnsureTable<SiteAuditCheckRunEntity>(SiteAuditCheckRunEntity.TableName);

            return Task.CompletedTask;
        }

        private void EnsureTable<TEntity>(string tableName)
        {
            if (TableExists(tableName)) return;

            Create.Table<TEntity>().Do();
        }
    }
}
