using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Common.Core.Models.Database
{
    [TableName("SeoToolkitSeoKeyValues")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class SeoKeyValueEntity
    {
        [PrimaryKeyColumn(AutoIncrement = false)]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Key")]
        public string Key { get; set; }

        [Column("Value")]
        public string Value { get; set; }

        [Column("DomainId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? DomainId { get; set; }
    }
}
