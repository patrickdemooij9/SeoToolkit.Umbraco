using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using SeoToolkit.Umbraco.uSync.Core.Serializers;
using uSync.Core.Serialization;
using uSync.Core.Tracking;

namespace SeoToolkit.Umbraco.uSync.Core.XmlTrackers;

public class SeoToolkitXmlTracker :
    SyncXmlTracker<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?>,
    ISyncTracker<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?>
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