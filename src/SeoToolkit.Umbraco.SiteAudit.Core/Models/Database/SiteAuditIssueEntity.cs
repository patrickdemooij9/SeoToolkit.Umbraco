using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    /// <summary>
    /// One finding.
    /// <para>
    /// The check is identified by its alias rather than a foreign key into a registry table.
    /// That removes the old behaviour of inserting a row the first time an unknown check
    /// appeared, lets a check from an add-on package produce results without touching the
    /// database at all, and makes an exported result readable on its own.
    /// </para>
    /// </summary>
    [TableName(TableName)]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditIssueEntity
    {
        public const string TableName = "SeoToolkitSiteAuditIssue";

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("RunId")]
        [ForeignKey(typeof(SiteAuditRunEntity), Column = "Id")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitSiteAuditIssue_RunId")]
        public int RunId { get; set; }

        /// <summary>Null for a finding about the site as a whole rather than one page.</summary>
        [Column("ResourceId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitSiteAuditIssue_ResourceId")]
        public int? ResourceId { get; set; }

        [Column("CheckAlias")]
        [Length(255)]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitSiteAuditIssue_CheckAlias")]
        public string CheckAlias { get; set; }

        /// <summary>Which flavour of failure this is, for example Missing or TooLong.</summary>
        [Column("Variant")]
        [Length(100)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Variant { get; set; }

        [Column("Severity")]
        public int Severity { get; set; }

        [Column("Url")]
        [Length(2048)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Url { get; set; }

        /// <summary>
        /// The values behind the message. Stored structured rather than pre-formatted, so the
        /// wording can be localised and the grid can filter on real values.
        /// </summary>
        [Column("DataJson")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DataJson { get; set; }

        [Column("Evidence")]
        [Length(1000)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Evidence { get; set; }

        /// <summary>Identity of this finding, used to recognise it again in a later run.</summary>
        [Column("IssueHash")]
        [Length(40)]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitSiteAuditIssue_IssueHash")]
        public string IssueHash { get; set; }
    }
}
