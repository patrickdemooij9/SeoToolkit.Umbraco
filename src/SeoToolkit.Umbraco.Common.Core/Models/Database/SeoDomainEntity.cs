using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Common.Core.Models.Database
{
    [TableName("SeoToolkitDomains")]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class SeoDomainEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("DomainId")]
        public int DomainId { get; set; }

        [Column("CollectionId")]
        [ForeignKey(typeof(SeoDomainCollectionEntity), Name = "FK_SeoToolkitDomains_SeoToolkitDomainCollections", Column = "Id")]
        [Index(IndexTypes.NonClustered, Name = "IX_SeoToolkitDomainsCollectionId")]
        public Guid CollectionId { get; set; }
    }
}
