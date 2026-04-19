using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Database
{
    [TableName("SeoToolkitSitemapContent")]
    [PrimaryKey("NodeKey", AutoIncrement = false)]
    public class SitemapContentEntity
    {
        [Column("NodeKey")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid NodeKey { get; set; }

        [Column("ExcludeFromSitemap")]
        public bool ExcludeFromSitemap { get; set; }

        [Column("HideFromSitemap")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public bool? HideFromSitemap { get; set; }

        [Column("ChangeFrequency")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ChangeFrequency { get; set; }

        [Column("Priority")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public double? Priority { get; set; }
    }
}
