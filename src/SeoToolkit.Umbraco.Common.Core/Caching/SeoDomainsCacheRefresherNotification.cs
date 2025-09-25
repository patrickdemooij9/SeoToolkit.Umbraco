using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Umbraco.Common.Core.Caching
{
    public class SeoDomainsCacheRefresherNotification : CacheRefresherNotification
    {
        public SeoDomainsCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
