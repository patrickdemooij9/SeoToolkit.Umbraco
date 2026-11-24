using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Sitemap.Core.Caching;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SitemapCacheClearNotificationHandlerTests
    {
        private const string CacheKey = SitemapCacheConstants.ItemsCacheKeyPrefix + "pages_root";
        private const string OtherCacheKey = "SomethingElse";

        private AppCaches _appCaches = null!;
        private SitemapCacheClearNotificationHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _appCaches = new AppCaches(new ObjectCacheAppCache(), NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));
            _appCaches.RuntimeCache.Insert(CacheKey, () => "cached");
            _appCaches.RuntimeCache.Insert(OtherCacheKey, () => "other");

            _handler = new SitemapCacheClearNotificationHandler(_appCaches);
        }

        [Test]
        public void PublicAccessChange_ClearsSitemapCache()
        {
            _handler.Handle(new PublicAccessCacheRefresherNotification(new object(), MessageType.RefreshAll));

            AssertOnlySitemapCacheCleared();
        }

        [Test]
        public void ContentTypeChange_ClearsSitemapCache()
        {
            _handler.Handle(new ContentTypeCacheRefresherNotification(new object(), MessageType.RefreshAll));

            AssertOnlySitemapCacheCleared();
        }

        [Test]
        public void LanguageChange_ClearsSitemapCache()
        {
            _handler.Handle(new LanguageCacheRefresherNotification(new object(), MessageType.RefreshAll));

            AssertOnlySitemapCacheCleared();
        }

        [Test]
        public void SitemapCacheRefresher_RefreshAll_ClearsSitemapCache()
        {
            var refresher = new SitemapCacheRefresher(_appCaches, Mock.Of<IEventAggregator>(), Mock.Of<ICacheRefresherNotificationFactory>());

            refresher.RefreshAll();

            AssertOnlySitemapCacheCleared();
        }

        private void AssertOnlySitemapCacheCleared()
        {
            Assert.That(_appCaches.RuntimeCache.Get(CacheKey), Is.Null);
            Assert.That(_appCaches.RuntimeCache.Get(OtherCacheKey), Is.EqualTo("other"));
        }
    }
}
