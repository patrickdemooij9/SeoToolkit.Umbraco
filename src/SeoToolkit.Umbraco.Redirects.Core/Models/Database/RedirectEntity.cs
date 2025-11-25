using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.Database
{
    [TableName("SeoToolkitRedirects")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class RedirectEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Key")]
        public Guid Key { get; set; }

        [Column("Domain")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? Domain { get; set; }

        [Column("CustomDomain")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CustomDomain { get; set; }

        [Column("IsRegex")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitRegex", ForColumns = "IsRegex")]
        public bool IsRegex { get; set; }

        [Column("IsEnabled")]
        public bool IsEnabled { get; set; }

        [Column("OldUrl")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitOldUrl", ForColumns = "OldUrl, IsEnabled")]
        public string OldUrl { get; set; }

        [Column("NewUrl")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string NewUrl { get; set; }

        [Column("NewNodeId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? NewNodeId { get; set; }

        [Column("NewNodeKey")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? NewNodeKey { get; set; }

        [Column("NewNodeCultureId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? NewNodeCultureId { get; set; }

        [Column("RedirectCode")]
        public int RedirectCode { get; set; }

        [Column("LastUpdated")]
        public DateTime LastUpdated { get; set; }
        
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }
    }
}
