using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Caching
{
    public class ScriptManagerCacheRefresherNotification : CacheRefresherNotification
    {
        public ScriptManagerCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
