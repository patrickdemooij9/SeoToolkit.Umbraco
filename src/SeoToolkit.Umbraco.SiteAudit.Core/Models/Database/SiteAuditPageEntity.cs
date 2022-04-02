using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    [TableName("SeoToolkitSiteAuditPage")]
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
