using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Umbraco.Sitemap.Core.Caching
{
    public class SitemapCacheRefresherNotification : CacheRefresherNotification
    {
        public SitemapCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
