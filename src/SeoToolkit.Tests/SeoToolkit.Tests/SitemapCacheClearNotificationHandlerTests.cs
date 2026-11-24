using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Sitemap.Core.Caching;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SitemapCacheClearNotificationHandlerTests
    {
        private const string CacheKey = SitemapCacheConstants.ItemsCacheKeyPrefix + "pages_root";

        private AppCaches _appCaches = null!;
        private Mock<IServerMessenger> _messenger = null!;
        private Mock<ICacheRefresher> _refresher = null!;
        private SitemapCacheClearNotificationHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _appCaches = new AppCaches(new ObjectCacheAppCache(), NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));
            _appCaches.RuntimeCache.Insert(CacheKey, () => "cached");

            _messenger = new Mock<IServerMessenger>();
            _refresher = new Mock<ICacheRefresher>();
            _refresher.SetupGet(r => r.RefresherUniqueId).Returns(SitemapCacheRefresher.CacheGuid);
            var distributedCache = new DistributedCache(_messenger.Object, new CacheRefresherCollection(() => new[] { _refresher.Object }));

            _handler = new SitemapCacheClearNotificationHandler(_appCaches, distributedCache);
        }

        [Test]
        public void PublicAccessChange_ClearsLocalCache()
        {
            _handler.Handle(new PublicAccessCacheRefresherNotification(new object(), MessageType.RefreshAll));

            Assert.That(_appCaches.RuntimeCache.Get(CacheKey), Is.Null);
        }

        [Test]
        public void ContentTypeChange_ClearsLocalCache()
        {
            _handler.Handle(new ContentTypeCacheRefresherNotification(new object(), MessageType.RefreshAll));

            Assert.That(_appCaches.RuntimeCache.Get(CacheKey), Is.Null);
        }

        [Test]
        public void SitemapContentChanged_RefreshesAllServers()
        {
            _handler.Handle(new SitemapContentChangedNotification(Guid.NewGuid()));

            _messenger.Verify(m => m.QueueRefreshAll(_refresher.Object), Times.Once);
        }

        [Test]
        public void SitemapPageSettingsSaved_RefreshesAllServers()
        {
            _handler.Handle(new SitemapPageSettingsSavedNotification(new SitemapPageSettings()));

            _messenger.Verify(m => m.QueueRefreshAll(_refresher.Object), Times.Once);
        }
    }
}
