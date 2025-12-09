using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Text;
using uSync.Core.Serialization;
using uSync.Core.Tracking;

namespace SeoToolkit.Umbraco.uSync.Trackers
{
    public class SitemapSettingsTracker : SyncXmlTrackAndMerger<SitemapPageSettings>, ISyncTracker<SitemapPageSettings>
    {
        public SitemapSettingsTracker(SyncSerializerCollection serializers) : base(serializers)
        {
        }

        public override List<TrackingItem> TrackingItems => [
            TrackingItem.Single(nameof(SitemapPageSettings.HideFromSitemap), $"Info/{nameof(SitemapPageSettings.HideFromSitemap)}"),
            TrackingItem.Single(nameof(SitemapPageSettings.ChangeFrequency), $"Info/{nameof(SitemapPageSettings.ChangeFrequency)}"),
            TrackingItem.Single(nameof(SitemapPageSettings.Priority), $"Info/{nameof(SitemapPageSettings.Priority)}"),
        ];
    }
}
