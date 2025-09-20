using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Common.Core.Models.Database
{
    [PrimaryKey("Id", AutoIncrement = true)]
    [TableName("SeoToolkitDomainSettings")]
    public class SeoDomainSettingEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("SettingKey")]
        public string Key { get; set; }

        [Column("SettingValue")]
        public string Value { get; set; }

        [Column("CollectionId")]
        [ForeignKey(typeof(SeoDomainCollectionEntity), Name = "FK_SeoToolkitDomainSettings_SeoToolkitDomainCollections", Column = "Id")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitCollectionId")]
        public int CollectionId { get; set; }
    }
}
