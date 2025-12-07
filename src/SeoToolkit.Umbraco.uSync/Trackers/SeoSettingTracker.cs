using uSync.Core.Serialization;
using uSync.Core.Tracking;

namespace SeoToolkit.Umbraco.uSync.Trackers
{
    public class SeoSettingTracker : SyncXmlTrackAndMerger<SeoSetting>, ISyncTracker<SeoSetting>
    {
        public SeoSettingTracker(SyncSerializerCollection serializers) : base(serializers)
        {
        }

        public override List<TrackingItem> TrackingItems => [
            TrackingItem.Single(nameof(SeoSetting.IsEnabled), $"Info/{nameof(SeoSetting.IsEnabled)}")
        ];
    }
}
