using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsSettings.Database
{
    [TableName("SeoToolkitMetaFieldsSettings")]
    [ExplicitColumns]
    [PrimaryKey("NodeId", AutoIncrement = false)]
    public class MetaFieldsSettingsEntity
    {
        [Column("NodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int NodeId { get; set; }

        [Column("Fields")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string Fields { get; set; }

        [Column("InheritanceId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? InheritanceId { get; set; }
    }
}
