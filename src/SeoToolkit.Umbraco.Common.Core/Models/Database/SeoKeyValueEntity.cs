using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Common.Core.Models.Database
{
    [TableName("SeoToolkitSeoKeyValues")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SeoKeyValueEntity
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Key")]
        public string Key { get; set; }

        [Column("Value")]
        public string Value { get; set; }

        [Column("DomainId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? DomainId { get; set; }
    }
}
