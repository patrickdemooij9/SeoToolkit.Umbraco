using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Sitemap.Core.Caching
{
    /// <summary>
    /// Clears the cached sitemap items on every server, so changes to SeoToolkit's own sitemap settings also reach
    /// other servers when load balancing.
    /// </summary>
    public sealed class SitemapCacheRefresher : CacheRefresherBase<SitemapCacheRefresherNotification>
    {
        public static readonly Guid CacheGuid = new("0d1c5c7e-4b8a-4f1e-9a52-6f3b8e2d7c41");

        public override Guid RefresherUniqueId => CacheGuid;
        public override string Name => "SeoToolkit Sitemap Cache Refresher";

        public SitemapCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearByKey(SitemapCacheConstants.ItemsCacheKeyPrefix);
            base.RefreshAll();
        }
    }
}
