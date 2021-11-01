using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    [TableName("uSeoToolkitSiteCheck")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SiteCheckEntity
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Alias")]
        public string Alias { get; set; }
    }
}
