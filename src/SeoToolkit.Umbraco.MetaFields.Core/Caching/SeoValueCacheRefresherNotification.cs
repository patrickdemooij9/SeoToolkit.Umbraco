using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Umbraco.MetaFields.Core.Caching
{
    public class SeoValueCacheRefresherNotification : CacheRefresherNotification
    {
        public SeoValueCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
