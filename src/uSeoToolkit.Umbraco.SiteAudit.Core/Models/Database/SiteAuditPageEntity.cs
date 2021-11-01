using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    [TableName("uSeoToolkitSiteAuditPage")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditPageEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("AuditId")]
        [ForeignKey(typeof(SiteAuditEntity), Column = "Id")]
        public int AuditId { get; set; }

        [Column("Url")]
        public string Url { get; set; }

        [Column("StatusCode")]
        public int StatusCode { get; set; }
    }
}
