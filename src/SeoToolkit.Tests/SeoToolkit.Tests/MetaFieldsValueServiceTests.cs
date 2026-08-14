using Moq;
using SeoToolkit.Umbraco.MetaFields.Core.Caching;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class MetaFieldsValueServiceTests
    {
        private const string Culture = "en-US";

        private Mock<IMetaFieldsValueRepository> _repository = null!;
        private Dictionary<string, object> _stored = null!;
        private MetaFieldsValueService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _stored = new Dictionary<string, object>();
            _repository = new Mock<IMetaFieldsValueRepository>();

            _repository.Setup(x => x.GetAllValues(It.IsAny<Guid>(), Culture))
                .Returns(() => new Dictionary<string, object>(_stored));
            _repository.Setup(x => x.Exists(It.IsAny<Guid>(), It.IsAny<string>(), Culture))
                .Returns((Guid _, string alias, string _) => _stored.ContainsKey(alias));
            _repository.Setup(x => x.Add(It.IsAny<Guid>(), It.IsAny<string>(), Culture, It.IsAny<object>()))
                .Callback((Guid _, string alias, string _, object value) => _stored[alias] = value);
            _repository.Setup(x => x.Update(It.IsAny<Guid>(), It.IsAny<string>(), Culture, It.IsAny<object>()))
                .Callback((Guid _, string alias, string _, object value) => _stored[alias] = value);

            var variationContextAccessor = new Mock<IVariationContextAccessor>();
            variationContextAccessor.Setup(x => x.VariationContext).Returns(new VariationContext(Culture));

            var appCaches = new AppCaches(
                new ObjectCacheAppCache(),
                NoAppCache.Instance,
                new IsolatedCaches(_ => new ObjectCacheAppCache()));

            _service = new MetaFieldsValueService(
                _repository.Object,
                variationContextAccessor.Object,
                appCaches,
                BuildDistributedCache(appCaches));
        }

        /// <summary>
        /// Mirrors what ServerMessengerBase does for local delivery, so the cache refresher
        /// actually runs instead of the message disappearing into a bare mock.
        /// </summary>
        private static DistributedCache BuildDistributedCache(AppCaches appCaches)
        {
            var refresher = new SeoValueCacheRefresher(
                appCaches,
                Mock.Of<IEventAggregator>(),
                Mock.Of<ICacheRefresherNotificationFactory>());

            var messenger = new Mock<IServerMessenger>();
            messenger.Setup(x => x.QueueRefresh(It.IsAny<ICacheRefresher>(), It.IsAny<Guid[]>()))
                .Callback((ICacheRefresher target, Guid[] ids) =>
                {
                    foreach (var id in ids)
                    {
                        target.Refresh(id);
                    }
                });

            return new DistributedCache(
                messenger.Object,
                new CacheRefresherCollection(() => new ICacheRefresher[] { refresher }));
        }

        [Test]
        public void AddValues_AfterAPreviousSave_ReturnsTheLatestValue()
        {
            var nodeId = Guid.NewGuid();

            _service.AddValues(nodeId, new Dictionary<string, object> { { "title", "hello world" } });
            Assert.That(_service.GetUserValues(nodeId)["title"], Is.EqualTo("hello world"));

            _service.AddValues(nodeId, new Dictionary<string, object> { { "title", "hello world again" } });

            // Regression for #563: the second edit used to be lost, leaving the first one in place.
            Assert.That(_service.GetUserValues(nodeId)["title"], Is.EqualTo("hello world again"));
        }

        [Test]
        public void AddValues_WhenTheValueAlreadyExists_UpdatesRatherThanInserts()
        {
            var nodeId = Guid.NewGuid();

            _service.AddValues(nodeId, new Dictionary<string, object> { { "title", "hello world" } });
            _service.AddValues(nodeId, new Dictionary<string, object> { { "title", "hello world again" } });

            _repository.Verify(x => x.Add(nodeId, "title", Culture, "hello world"), Times.Once);
            _repository.Verify(x => x.Update(nodeId, "title", Culture, "hello world again"), Times.Once);
        }

        [Test]
        public void AddValues_WithANullValue_ClearsThePreviouslyStoredValue()
        {
            var nodeId = Guid.NewGuid();

            _service.AddValues(nodeId, new Dictionary<string, object> { { "title", "hello world" } });
            _service.AddValues(nodeId, new Dictionary<string, object> { { "title", null! } });

            Assert.That(_service.GetUserValues(nodeId)["title"], Is.Null);
        }
    }
}
