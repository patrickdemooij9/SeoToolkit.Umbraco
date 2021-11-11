using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    [TableName("uSeoToolkitSiteAuditCheck")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditCheckEntity
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey(typeof(SiteAuditEntity), Column = "Id", Name = "FK_AuditCheck_Audit")]
        [Column("AuditId")]
        public int AuditId { get; set; }

        [ForeignKey(typeof(SiteCheckEntity), Column = "Id", Name = "FK_AuditCheck_Check")]
        [Column("CheckId")]
        public int CheckId { get; set; }
    }
}
