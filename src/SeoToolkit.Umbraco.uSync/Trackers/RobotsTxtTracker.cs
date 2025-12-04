using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Text;
using uSync.Core.Serialization;
using uSync.Core.Tracking;

namespace SeoToolkit.Umbraco.uSync.Trackers
{
    /*public class RobotsTxtTracker : SyncXmlTrackAndMerger<RobotsTxtModel>, ISyncTracker<RobotsTxtModel>
    {
        public RobotsTxtTracker(SyncSerializerCollection serializers) : base(serializers)
        {
        }

        public override List<TrackingItem> TrackingItems => [
            TrackingItem.Single(nameof(RobotsTxtModel.Content), $"Info/{nameof(RobotsTxtModel.Content)}"),
            TrackingItem.Single(nameof(RobotsTxtModel.DomainId), $"Info/{nameof(RobotsTxtModel.DomainId)}"),
        ];
    }*/
}
