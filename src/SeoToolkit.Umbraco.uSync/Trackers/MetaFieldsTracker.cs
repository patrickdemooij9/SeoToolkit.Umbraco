using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Text;
using uSync.Core.Serialization;
using uSync.Core.Tracking;

namespace SeoToolkit.Umbraco.uSync.Trackers
{
    public class MetaFieldsTracker : SyncXmlTrackAndMerger<DocumentTypeSettingsDto>, ISyncTracker<DocumentTypeSettingsDto>
    {
        public MetaFieldsTracker(SyncSerializerCollection serializers) : base(serializers)
        {
        }
    }
}
