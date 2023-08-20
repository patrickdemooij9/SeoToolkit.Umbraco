using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Umbraco.Common.Core.Caching
{
    public class SeoSettingsCacheRefresherNotification : CacheRefresherNotification
    {
        public SeoSettingsCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
