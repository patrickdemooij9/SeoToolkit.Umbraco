using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Umbraco.Redirects.Core.Caching
{
    public class RedirectsCacheRefresherNotification : CacheRefresherNotification
    {
        public RedirectsCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
