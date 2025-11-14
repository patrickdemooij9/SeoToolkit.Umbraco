using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Models.Database
{
    [TableName("SeoToolkitRobotsTxt")]
    [PrimaryKey("Key", AutoIncrement = false)]
    public class RobotsTxtEntity
    {
        [Column("Id")]
        public int Id { get; set; }

        [Column("Key")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Key { get; set; }

        [Column("Content")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string Content { get; set; }

        [Column("DomainId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? DomainId { get; set; }
    }
}
