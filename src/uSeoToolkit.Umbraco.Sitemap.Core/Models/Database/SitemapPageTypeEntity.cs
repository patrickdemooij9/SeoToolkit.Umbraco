using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Models.Database
{
    [TableName("uSeoToolkitSitemapPageType")]
    [PrimaryKey("ContentTypeId", AutoIncrement = false)]
    public class SitemapPageTypeEntity
    {
        [Column("ContentTypeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int ContentTypeId { get; set; }

        [Column("HideFromSitemap")]
        public bool HideFromSitemap { get; set; }

        [Column("ChangeFrequency")]
        public string ChangeFrequency { get; set; }

        [Column("Priority")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public double? Priority { get; set; }
    }
}
