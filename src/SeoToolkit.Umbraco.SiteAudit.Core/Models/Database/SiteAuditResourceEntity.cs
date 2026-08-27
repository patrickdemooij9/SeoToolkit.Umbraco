using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    /// <summary>
    /// One crawled resource.
    /// <para>
    /// The issue counts are denormalised on purpose: the pages grid sorts and filters on them,
    /// and without them every row would need a join and a group-by against a table with an order
    /// of magnitude more rows.
    /// </para>
    /// </summary>
    [TableName(TableName)]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditResourceEntity
    {
        public const string TableName = "SeoToolkitSiteAuditResource";

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("RunId")]
        [ForeignKey(typeof(SiteAuditRunEntity), Column = "Id")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitSiteAuditResource_RunId")]
        public int RunId { get; set; }

        /// <summary>Hash of the normalised url, which is what lookups go through.</summary>
        [Column("UrlHash")]
        [Length(40)]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitSiteAuditResource_UrlHash")]
        public string UrlHash { get; set; }

        [Column("Url")]
        [Length(2048)]
        public string Url { get; set; }

        [Column("FinalUrl")]
        [Length(2048)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string FinalUrl { get; set; }

        [Column("StatusCode")]
        public int StatusCode { get; set; }

        [Column("ContentType")]
        [Length(255)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ContentType { get; set; }

        [Column("KindId")]
        public int KindId { get; set; }

        [Column("Depth")]
        public int Depth { get; set; }

        [Column("IsInternal")]
        public bool IsInternal { get; set; }

        [Column("ResponseTimeMs")]
        public int ResponseTimeMs { get; set; }

        [Column("SizeBytes")]
        public long SizeBytes { get; set; }

        [Column("RedirectCount")]
        public int RedirectCount { get; set; }

        [Column("Title")]
        [Length(1000)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Title { get; set; }

        [Column("MetaDescription")]
        [Length(1000)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string MetaDescription { get; set; }

        [Column("H1")]
        [Length(1000)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string H1 { get; set; }

        [Column("WordCount")]
        public int WordCount { get; set; }

        [Column("IndexabilityFlags")]
        public int IndexabilityFlags { get; set; }

        [Column("FailureReasonId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? FailureReasonId { get; set; }

        /// <summary>Set once the crawled url has been matched back to a node in the content tree.</summary>
        [Column("UmbracoContentKey")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? UmbracoContentKey { get; set; }

        [Column("Culture")]
        [Length(20)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Culture { get; set; }

        [Column("IssueCount")]
        public int IssueCount { get; set; }

        [Column("ErrorCount")]
        public int ErrorCount { get; set; }

        [Column("WarningCount")]
        public int WarningCount { get; set; }
    }
}
