using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database
{
    [TableName("uSeoToolkitSeoValue")]
    [ExplicitColumns]
    [PrimaryKey(new[] { "NodeId", "Alias" })]
    public class SeoValueEntity
    {
        [Column("NodeId")]
        [PrimaryKeyColumn(AutoIncrement = false, OnColumns = "NodeId, Alias")]
        public int NodeId { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }

        [Column("UserValue")]
        public string UserValue { get; set; }
    }
}
