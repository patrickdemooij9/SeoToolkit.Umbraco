using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Sitemap.Core.Caching
{
    /// <summary>
    /// Clears the cached sitemap items whenever something changes that affects which pages are in the sitemap.
    /// Umbraco's cache refresher notifications are already published on every server, so those clear the local cache.
    /// SeoToolkit's own notifications are only published on the server that handled the save, so those go through
    /// <see cref="SitemapCacheRefresher"/> to reach every server.
    /// </summary>
    public class SitemapCacheClearNotificationHandler :
        INotificationHandler<ContentCacheRefresherNotification>,
        INotificationHandler<DomainCacheRefresherNotification>,
        INotificationHandler<PublicAccessCacheRefresherNotification>,
        INotificationHandler<ContentTypeCacheRefresherNotification>,
        INotificationHandler<LanguageCacheRefresherNotification>,
        INotificationHandler<SitemapContentChangedNotification>,
        INotificationHandler<SitemapPageSettingsSavedNotification>
    {
        private readonly AppCaches _appCaches;
        private readonly DistributedCache _distributedCache;

        public SitemapCacheClearNotificationHandler(AppCaches appCaches, DistributedCache distributedCache)
        {
            _appCaches = appCaches;
            _distributedCache = distributedCache;
        }

        public void Handle(ContentCacheRefresherNotification notification) => ClearLocalCache();

        public void Handle(DomainCacheRefresherNotification notification) => ClearLocalCache();

        public void Handle(PublicAccessCacheRefresherNotification notification) => ClearLocalCache();

        public void Handle(ContentTypeCacheRefresherNotification notification) => ClearLocalCache();

        public void Handle(LanguageCacheRefresherNotification notification) => ClearLocalCache();

        public void Handle(SitemapContentChangedNotification notification) => ClearCacheOnAllServers();

        public void Handle(SitemapPageSettingsSavedNotification notification) => ClearCacheOnAllServers();

        private void ClearLocalCache()
        {
            _appCaches.RuntimeCache.ClearByKey(SitemapCacheConstants.ItemsCacheKeyPrefix);
        }

        private void ClearCacheOnAllServers()
        {
            _distributedCache.RefreshAll(SitemapCacheRefresher.CacheGuid);
        }
    }
}
