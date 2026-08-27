using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    /// <summary>
    /// Per-check totals for a run.
    /// <para>
    /// This is what makes both the summary view and the health score cheap: neither ever has to
    /// load pages back out just to count how often a check failed. It also records how many
    /// resources a check was applicable to, which is the denominator the score needs - without
    /// it, disabling a check would move the score for the wrong reason.
    /// </para>
    /// </summary>
    [TableName(TableName)]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditCheckRunEntity
    {
        public const string TableName = "SeoToolkitSiteAuditCheckRun";

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("RunId")]
        [ForeignKey(typeof(SiteAuditRunEntity), Column = "Id")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitSiteAuditCheckRun_RunId")]
        public int RunId { get; set; }

        [Column("CheckAlias")]
        [Length(255)]
        public string CheckAlias { get; set; }

        [Column("CheckName")]
        [Length(255)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CheckName { get; set; }

        [Column("CategoryId")]
        public int CategoryId { get; set; }

        /// <summary>False when the check was skipped, for example because a feature is not installed.</summary>
        [Column("DidRun")]
        public bool DidRun { get; set; }

        /// <summary>How many resources this check was applicable to.</summary>
        [Column("ApplicableCount")]
        public int ApplicableCount { get; set; }

        /// <summary>How many of those it found a problem with.</summary>
        [Column("FailedCount")]
        public int FailedCount { get; set; }

        [Column("IssueCount")]
        public int IssueCount { get; set; }

        [Column("Weight")]
        public int Weight { get; set; }

        [Column("Severity")]
        public int Severity { get; set; }

        [Column("DurationMs")]
        public long DurationMs { get; set; }
    }
}
