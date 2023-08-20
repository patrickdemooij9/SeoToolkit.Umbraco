using SeoToolkit.Umbraco.Redirects.Core.Constants;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Redirects.Core.Caching
{
    public sealed class RedirectsCacheRefresher : CacheRefresherBase<RedirectsCacheRefresherNotification>
    {
        public static Guid CacheGuid = new("3c46284f-3fad-49fe-9cad-f436f0762c5d");

        public override Guid RefresherUniqueId => CacheGuid;
        public override string Name => "Redirects Cache Refresher";

        public RedirectsCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearByKey(CacheConstants.Redirects);
            base.RefreshAll();
        }
    }
}
