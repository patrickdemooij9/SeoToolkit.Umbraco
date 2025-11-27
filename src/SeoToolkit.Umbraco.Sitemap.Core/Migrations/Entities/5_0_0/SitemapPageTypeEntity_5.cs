using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Migrations.Entities._5_0_0
{
    [TableName("SeoToolkitSitemapPageType")]
    [PrimaryKey("ContentTypeId", AutoIncrement = false)]
    internal class SitemapPageTypeEntity_5
    {
        [Column("ContentTypeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int ContentTypeId { get; set; }

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
