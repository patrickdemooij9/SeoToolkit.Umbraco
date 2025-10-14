using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Common.Core.Models.Database
{
    [TableName("SeoToolkitDomains")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SeoDomainEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("DomainId")]
        public int DomainId { get; set; }

        [Column("CollectionId")]
        [ForeignKey(typeof(SeoDomainCollectionEntity), Name = "FK_SeoToolkitDomains_SeoToolkitDomainCollections", Column = "Id")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitDomainsCollectionId")]
        public int CollectionId { get; set; }
    }
}
