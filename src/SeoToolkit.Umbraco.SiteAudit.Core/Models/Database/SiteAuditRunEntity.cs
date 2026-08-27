using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    /// <summary>
    /// One audit run. Replaces SeoToolkitSiteAudit, which carried only the request settings -
    /// everything about the outcome had to be recomputed by loading every page back out.
    /// The counts and score live here so the overview never has to touch the results tables.
    /// </summary>
    [TableName(TableName)]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditRunEntity
    {
        public const string TableName = "SeoToolkitSiteAuditRun";

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        /// <summary>Stable external identifier. The old table had none, so Get(Guid) simply threw.</summary>
        [Column("Key")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_SeoToolkitSiteAuditRun_Key")]
        public Guid Key { get; set; }

        [Column("Name")]
        [Length(255)]
        public string Name { get; set; }

        [Column("StatusId")]
        public int StatusId { get; set; }

        [Column("CreatedUtc")]
        public DateTime CreatedUtc { get; set; }

        [Column("StartedUtc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? StartedUtc { get; set; }

        [Column("FinishedUtc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? FinishedUtc { get; set; }

        [Column("StartingUrl")]
        [Length(2048)]
        public string StartingUrl { get; set; }

        /// <summary>The decoupled frontend this run was retargeted to, when there is one.</summary>
        [Column("BaseUrl")]
        [Length(500)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string BaseUrl { get; set; }

        [Column("MaxPages")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? MaxPages { get; set; }

        [Column("MaxDepth")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? MaxDepth { get; set; }

        [Column("DelayMs")]
        public int DelayMs { get; set; }

        [Column("Concurrency")]
        public int Concurrency { get; set; }

        [Column("UserAgent")]
        [Length(255)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string UserAgent { get; set; }

        [Column("TotalDiscovered")]
        public int TotalDiscovered { get; set; }

        [Column("TotalCrawled")]
        public int TotalCrawled { get; set; }

        [Column("CriticalCount")]
        public int CriticalCount { get; set; }

        [Column("ErrorCount")]
        public int ErrorCount { get; set; }

        [Column("WarningCount")]
        public int WarningCount { get; set; }

        [Column("NoticeCount")]
        public int NoticeCount { get; set; }

        [Column("Score")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Score { get; set; }

        /// <summary>
        /// Which scoring formula produced <see cref="Score"/>. Persisted so a future change to
        /// the formula cannot silently corrupt historical trends.
        /// </summary>
        [Column("ScoreVersion")]
        public int ScoreVersion { get; set; }

        /// <summary>Last sign of life from the server running this. A stale one means it died.</summary>
        [Column("HeartbeatUtc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? HeartbeatUtc { get; set; }

        /// <summary>Which server picked this run up, so a load balanced setup cannot double-run it.</summary>
        [Column("ClaimedBy")]
        [Length(255)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ClaimedBy { get; set; }

        /// <summary>The full crawl configuration, so a run can be repeated exactly.</summary>
        [Column("ConfigJson")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ConfigJson { get; set; }

        /// <summary>Set when an add-on scheduled this run rather than a person starting it.</summary>
        [Column("ScheduleId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? ScheduleId { get; set; }
    }
}
