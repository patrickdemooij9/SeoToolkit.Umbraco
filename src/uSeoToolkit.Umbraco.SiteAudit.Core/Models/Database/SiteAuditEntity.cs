using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    [TableName("uSeoToolkitSiteAudit")]
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

        [Column("StartingNodeId")]
        public int StartingNodeId { get; set; }

        [Column("MaxPagesToCrawl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? MaxPagesToCrawl { get; set; }

        [Column("DelayBetweenRequests")]
        public int DelayBetweenRequests { get; set; }
        
        [Column("TotalPagesFound")]
        public int TotalPagesFound { get; set; }
    }
}
