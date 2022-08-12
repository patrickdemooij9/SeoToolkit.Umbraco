using System.Xml.Linq;
using SeoToolkit.Umbraco.uSync.Core.Serializers;
using Umbraco.Cms.Core.Models;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;
using uSync.Core.Tracking;
using uSync.Core.Tracking.Impliment;

namespace SeoToolkit.Umbraco.uSync.Core.XmlTrackers;

public class SeoToolkitXmlTracker :
    SyncXmlTracker<IContent>,
    ISyncTracker<IContent>,
    ISyncTrackerBase
{
    public override List<TrackingItem> TrackingItems => new List<TrackingItem>()
    {
        TrackingItem.Many("Property - *", "/*/Value", "@Culture"),

    };

    public SeoToolkitXmlTracker(SyncSerializerCollection serializers, MetaFieldValuesSerializer serializer) : base(serializers)
    {
        this.serializer = serializer;
    }

   
}