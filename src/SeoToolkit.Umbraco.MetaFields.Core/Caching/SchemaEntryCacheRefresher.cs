using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Umbraco.MetaFields.Core.Caching
{
    /// <summary>
    /// Clears the cached schema entries on every server.
    /// </summary>
    public sealed class SchemaEntryCacheRefresher : CacheRefresherBase<SchemaEntryCacheRefresherNotification>
    {
        public static readonly Guid CacheGuid = new("9c2e5b71-3a48-4d6f-b0e1-6a8d2c4f7e35");

        public SchemaEntryCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override Guid RefresherUniqueId => CacheGuid;

        public override string Name => "Seo Schema Entry Cache Refresher";

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearByKey(CacheConstants.SchemaEntry);
            base.RefreshAll();
        }
    }

    public class SchemaEntryCacheRefresherNotification : CacheRefresherNotification
    {
        public SchemaEntryCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
