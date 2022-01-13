using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Models.Database
{
    [TableName("uSeoToolkitRobotsTxt")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class RobotsTxtEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Content")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string Content { get; set; }
    }
}
