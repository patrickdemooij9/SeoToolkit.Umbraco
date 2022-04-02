using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.Database
{
    [TableName("SeoToolkitScript")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class ScriptEntity
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("DefinitionAlias")]
        public string DefinitionAlias { get; set; }

        [Column("Config")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string Config { get; set; }
    }
}
