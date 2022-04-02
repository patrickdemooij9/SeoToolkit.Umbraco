using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database
{
    [TableName("SeoToolkitMetaFieldsValue")]
    [ExplicitColumns]
    [PrimaryKey(new[] { "NodeId", "Alias", "Culture" })]
    public class MetaFieldsValueEntity
    {
        [Column("NodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, OnColumns = "NodeId, Alias, Culture")]
        public int NodeId { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("Culture")]
        public string Culture { get; set; } = "";

        [Column("UserValue")]
        public string UserValue { get; set; }
    }
}
