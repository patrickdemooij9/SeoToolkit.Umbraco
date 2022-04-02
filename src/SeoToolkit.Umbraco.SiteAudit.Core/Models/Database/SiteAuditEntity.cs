using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    [TableName("SeoToolkitSiteAudit")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteAuditEntity
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("StatusId")]
        public int StatusId { get; set; }

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("StartingUrl")]
        public string StartingUrl { get; set; }

        [Column("MaxPagesToCrawl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? MaxPagesToCrawl { get; set; }

        [Column("DelayBetweenRequests")]
        public int DelayBetweenRequests { get; set; }
        
        [Column("TotalPagesFound")]
        public int TotalPagesFound { get; set; }
    }
}
