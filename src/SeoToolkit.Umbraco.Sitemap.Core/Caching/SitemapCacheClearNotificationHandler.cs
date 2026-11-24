using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Sitemap.Core.Caching
{
    /// <summary>
    /// Clears the cached sitemap items whenever content, domains or sitemap settings change.
    /// The cache refresher notifications are published on every server, so this also works when load balancing.
    /// </summary>
    public class SitemapCacheClearNotificationHandler :
        INotificationHandler<ContentCacheRefresherNotification>,
        INotificationHandler<DomainCacheRefresherNotification>,
        INotificationHandler<SitemapContentChangedNotification>,
        INotificationHandler<SitemapPageSettingsSavedNotification>
    {
        private readonly AppCaches _appCaches;

        public SitemapCacheClearNotificationHandler(AppCaches appCaches)
        {
            _appCaches = appCaches;
        }

        public void Handle(ContentCacheRefresherNotification notification) => ClearCache();

        public void Handle(DomainCacheRefresherNotification notification) => ClearCache();

        public void Handle(SitemapContentChangedNotification notification) => ClearCache();

        public void Handle(SitemapPageSettingsSavedNotification notification) => ClearCache();

        private void ClearCache()
        {
            _appCaches.RuntimeCache.ClearByKey(SitemapCacheConstants.ItemsCacheKeyPrefix);
        }
    }
}
