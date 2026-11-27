using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Caching;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class CachedSeoKeyValueRepositoryTests
    {
        private const string Key = "title";

        private Mock<ISeoKeyValueRepository> _inner = null!;
        private Mock<IServerMessenger> _messenger = null!;
        private SeoKeyValuesCacheRefresher _refresher = null!;
        private CachedSeoKeyValueRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var appCaches = new AppCaches(new ObjectCacheAppCache(), NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));
            _refresher = new SeoKeyValuesCacheRefresher(appCaches, Mock.Of<IEventAggregator>(), Mock.Of<ICacheRefresherNotificationFactory>());

            // Delivers the refresh to this server, as the real messenger does alongside queueing it for the others.
            _messenger = new Mock<IServerMessenger>();
            _messenger.Setup(it => it.QueueRefresh(It.IsAny<ICacheRefresher>(), It.IsAny<Guid[]>()))
                .Callback<ICacheRefresher, Guid[]>((refresher, ids) =>
                {
                    foreach (var id in ids) refresher.Refresh(id);
                });
            _messenger.Setup(it => it.QueueRefreshAll(It.IsAny<ICacheRefresher>()))
                .Callback<ICacheRefresher>(refresher => refresher.RefreshAll());

            _inner = new Mock<ISeoKeyValueRepository>();
            _inner.Setup(it => it.Get(It.IsAny<Guid?>())).Returns(() => new Dictionary<string, string> { [Key] = "value" });

            _repository = new CachedSeoKeyValueRepository(
                _inner.Object,
                appCaches,
                new DistributedCache(_messenger.Object, new CacheRefresherCollection(() => [_refresher])));
        }

        [Test]
        public void Reads_AreServedFromTheCache()
        {
            Assert.That(_repository.Get(Key, null), Is.EqualTo("value"));
            Assert.That(_repository.Get((Guid?)null)[Key], Is.EqualTo("value"));
            Assert.That(_repository.Get("missing", null), Is.Null);

            _inner.Verify(it => it.Get(null), Times.Once);
        }

        [Test]
        public void Domains_AreCachedSeparately()
        {
            var domainId = Guid.NewGuid();

            _repository.Get(Key, null);
            _repository.Get(Key, domainId);
            _repository.Get(Key, domainId);

            _inner.Verify(it => it.Get(null), Times.Once);
            _inner.Verify(it => it.Get(domainId), Times.Once);
        }

        [Test]
        public void Get_ReturnsACopy()
        {
            _repository.Get((Guid?)null)[Key] = "changed";

            Assert.That(_repository.Get(Key, null), Is.EqualTo("value"));
        }

        [Test]
        public void Set_WritesAndRefreshesTheDomainOnEveryServer()
        {
            var domainId = Guid.NewGuid();
            _repository.Get(Key, domainId);
            _repository.Get(Key, null);

            _repository.Set(Key, "new", domainId);

            _inner.Verify(it => it.Set(Key, "new", domainId), Times.Once);
            _messenger.Verify(it => it.QueueRefresh(_refresher, new[] { domainId }), Times.Once);

            _repository.Get(Key, domainId);
            _repository.Get(Key, null);
            _inner.Verify(it => it.Get(domainId), Times.Exactly(2));
            _inner.Verify(it => it.Get(null), Times.Once);
        }

        [Test]
        public void Delete_OfARootValue_WritesAndRefreshesAllOnEveryServer()
        {
            _repository.Get(Key, null);

            _repository.Delete(Key, null);

            _inner.Verify(it => it.Delete(Key, null), Times.Once);
            _messenger.Verify(it => it.QueueRefreshAll(_refresher), Times.Once);

            _repository.Get(Key, null);
            _inner.Verify(it => it.Get(null), Times.Exactly(2));
        }

        [Test]
        public void RefreshAll_ClearsEveryDomain()
        {
            var domainId = Guid.NewGuid();
            _repository.Get(Key, null);
            _repository.Get(Key, domainId);

            _refresher.RefreshAll();

            _repository.Get(Key, null);
            _repository.Get(Key, domainId);
            _inner.Verify(it => it.Get(null), Times.Exactly(2));
            _inner.Verify(it => it.Get(domainId), Times.Exactly(2));
        }
    }
}
