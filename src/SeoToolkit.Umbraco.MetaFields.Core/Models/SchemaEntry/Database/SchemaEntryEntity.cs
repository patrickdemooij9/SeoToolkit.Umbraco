using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Database
{
    [TableName("SeoToolkitSchemaEntry")]
    [ExplicitColumns]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class SchemaEntryEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("OwnerType")]
        public string OwnerType { get; set; }

        [Column("OwnerKey")]
        public Guid OwnerKey { get; set; }

        [Column("SchemaAlias")]
        public string SchemaAlias { get; set; }

        [Column("DisplayName")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(500)]
        public string DisplayName { get; set; }

        [Column("PropertiesJson")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string PropertiesJson { get; set; }
    }
}
