using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database
{
    [TableName("SeoToolkitMetaFieldsValue")]
    [ExplicitColumns]
    [PrimaryKey(new[] { "NodeKey", "Alias", "Culture" })]
    public class MetaFieldsValueEntity
    {
        [Column("NodeId")]
        public int NodeId { get; set; }

        [Column("NodeKey")]
        [PrimaryKeyColumn(AutoIncrement = false, OnColumns = "NodeKey, Alias, Culture")]
        public Guid NodeKey { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("Culture")]
        public string Culture { get; set; } = "";

        [Column("UserValue")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string UserValue { get; set; }
    }
}
