using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Database
{
    [TableName("SeoToolkitMetaFieldsSettings")]
    [ExplicitColumns]
    [PrimaryKey("NodeKey", AutoIncrement = false)]
    public class MetaFieldsSettingsEntity
    {
        [Column("NodeId")]
        public int NodeId { get; set; }

        [Column("NodeKey")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid NodeKey { get; set; }

        [Column("Fields")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string Fields { get; set; }

        [Column("InheritanceId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? InheritanceId { get; set; }

        [Column("InheritanceKey")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? InheritanceKey { get; set; }
    }
}
