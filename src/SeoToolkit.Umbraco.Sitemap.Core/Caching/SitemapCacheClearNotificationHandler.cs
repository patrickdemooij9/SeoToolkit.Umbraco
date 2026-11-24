using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Sitemap.Core.Caching
{
    /// <summary>
    /// Clears the cached sitemap items whenever Umbraco data changes that affects which pages are in the sitemap.
    /// These cache refresher notifications are published on every server, so clearing the local cache is enough.
    /// Changes to SeoToolkit's own sitemap settings are cleared by the SitemapService through <see cref="SitemapCacheRefresher"/>.
    /// </summary>
    public class SitemapCacheClearNotificationHandler :
        INotificationHandler<ContentCacheRefresherNotification>,
        INotificationHandler<DomainCacheRefresherNotification>,
        INotificationHandler<PublicAccessCacheRefresherNotification>,
        INotificationHandler<ContentTypeCacheRefresherNotification>,
        INotificationHandler<LanguageCacheRefresherNotification>
    {
        private readonly AppCaches _appCaches;

        public SitemapCacheClearNotificationHandler(AppCaches appCaches)
        {
            _appCaches = appCaches;
        }

        public void Handle(ContentCacheRefresherNotification notification) => ClearCache();

        public void Handle(DomainCacheRefresherNotification notification) => ClearCache();

        public void Handle(PublicAccessCacheRefresherNotification notification) => ClearCache();

        public void Handle(ContentTypeCacheRefresherNotification notification) => ClearCache();

        public void Handle(LanguageCacheRefresherNotification notification) => ClearCache();

        private void ClearCache()
        {
            _appCaches.RuntimeCache.ClearByKey(SitemapCacheConstants.ItemsCacheKeyPrefix);
        }
    }
}
