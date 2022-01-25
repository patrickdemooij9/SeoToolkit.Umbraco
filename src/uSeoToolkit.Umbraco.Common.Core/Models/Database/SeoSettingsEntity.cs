using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.Common.Core.Models.Database
{
    [TableName("uSeoToolkitSeoSettings")]
    [PrimaryKey("ContentTypeId", AutoIncrement = false)]
    public class SeoSettingsEntity
    {
        [Column("ContentTypeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int ContentTypeId { get; set; }

        [Column("Enabled")]
        public bool Enabled { get; set; }
    }
}
