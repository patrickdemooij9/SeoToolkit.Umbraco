using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Common.Core.Models.Database
{
    [PrimaryKey("Id", AutoIncrement = false)]
    [TableName("SeoToolkitDomainCollections")]
    public class SeoDomainCollectionEntity
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }
    }
}
