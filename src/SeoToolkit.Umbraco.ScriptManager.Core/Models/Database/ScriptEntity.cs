using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.Database
{
    [TableName("SeoToolkitScript")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class ScriptEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Key")]
        public Guid Key {get; set;}

        [Column("Name")]
        public string Name { get; set; }

        [Column("DefinitionAlias")]
        public string DefinitionAlias { get; set; }

        [Column("Config")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string Config { get; set; }

        [Column("DomainId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? DomainId { get; set; }
    }
}
