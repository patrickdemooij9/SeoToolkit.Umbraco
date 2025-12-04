using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using uSync.Core.Serialization;
using uSync.Core.Tracking;

namespace SeoToolkit.Umbraco.uSync.Trackers
{
    /*public class RedirectTracker : SyncXmlTrackAndMerger<Redirect>, ISyncTracker<Redirect>
    {
        public RedirectTracker(SyncSerializerCollection serializers) : base(serializers)
        {
        }

        public override List<TrackingItem> TrackingItems => [
            TrackingItem.Single(nameof(Redirect.IsEnabled), $"Info/{nameof(Redirect.IsEnabled)}"),
            TrackingItem.Single(nameof(Redirect.IsRegex), $"Info/{nameof(Redirect.IsRegex)}"),
            TrackingItem.Single(nameof(Redirect.Domain), $"Info/{nameof(Redirect.Domain)}"),
            TrackingItem.Single(nameof(Redirect.CustomDomain), $"Info/{nameof(Redirect.CustomDomain)}"),
            TrackingItem.Single(nameof(Redirect.OldUrl), $"Info/{nameof(Redirect.OldUrl)}"),
            TrackingItem.Single(nameof(Redirect.NewUrl), $"Info/{nameof(Redirect.NewUrl)}"),
            TrackingItem.Single(nameof(Redirect.NewNode), $"Info/{nameof(Redirect.NewNode)}"),
            TrackingItem.Single(nameof(Redirect.NewNodeCulture), $"Info/{nameof(Redirect.NewNodeCulture)}"),
            TrackingItem.Single(nameof(Redirect.CreatedBy), $"Info/{nameof(Redirect.CreatedBy)}"),
            TrackingItem.Single(nameof(Redirect.RedirectCode), $"Info/{nameof(Redirect.RedirectCode)}"),
        ];
    }*/
}
