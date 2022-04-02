using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    [TableName("SeoToolkitSiteAuditCheckResult")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditCheckResultEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("PageId")]
        [ForeignKey(typeof(SiteAuditPageEntity), Column = "Id")]
        public int PageId { get; set; }

        [Column("CheckId")]
        [ForeignKey(typeof(SiteCheckEntity), Column = "Id")]
        public int CheckId { get; set; }

        [Column("ResultId")]
        public int ResultId { get; set; }

        //Json object containing any extra values of the result
        [Column("ExtraValues")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string ExtraValues { get; set; }
    }
}
