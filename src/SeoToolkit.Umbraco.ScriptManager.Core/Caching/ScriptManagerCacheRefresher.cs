using SeoToolkit.Umbraco.ScriptManager.Core.Constants;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Caching
{
    public sealed class ScriptManagerCacheRefresher : CacheRefresherBase<ScriptManagerCacheRefresherNotification>
    {
        public static Guid CacheGuid = new("70b9222a-db9c-4da3-9c3a-63a8174cf6d8");

        public override Guid RefresherUniqueId => CacheGuid;
        public override string Name => "Script Manager Cache Refresher";

        public ScriptManagerCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearByKey(CacheConstants.ScriptManager);
            base.RefreshAll();
        }
    }
}
