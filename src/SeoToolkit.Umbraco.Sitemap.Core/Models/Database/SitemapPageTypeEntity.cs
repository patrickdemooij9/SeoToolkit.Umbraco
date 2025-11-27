using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Database
{
    [TableName("SeoToolkitSitemapPageType")]
    [PrimaryKey("ContentTypeGuid", AutoIncrement = false)]
    public class SitemapPageTypeEntity
    {
        [Column("ContentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("ContentTypeGuid")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid ContentTypeGuid { get; set; }

        [Column("HideFromSitemap")]
        public bool HideFromSitemap { get; set; }

        [Column("ChangeFrequency")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ChangeFrequency { get; set; }

        [Column("Priority")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public double? Priority { get; set; }
    }
}