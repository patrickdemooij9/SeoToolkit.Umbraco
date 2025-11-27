using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Migrations.Entities._5_0_0
{
    [TableName("SeoToolkitMetaFieldsValue")]
    [ExplicitColumns]
    [PrimaryKey(new[] { "NodeId", "Alias", "Culture" })]
    internal class MetaFieldsValueEntity_5
    {
        [Column("NodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, OnColumns = "NodeId, Alias, Culture")]
        public int NodeId { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("Culture")]
        public string Culture { get; set; } = "";

        [Column("UserValue")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string UserValue { get; set; }
    }
}
