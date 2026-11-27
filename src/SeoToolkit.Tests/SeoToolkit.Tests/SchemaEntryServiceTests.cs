using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.MetaFields.Core.Caching;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SchemaEntryRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SchemaEntryServiceTests
    {
        private const string OwnerType = "documentType";

        private Mock<ISchemaEntryRepository> _repository = null!;
        private Mock<IServerMessenger> _messenger = null!;
        private SchemaEntryCacheRefresher _refresher = null!;
        private SchemaEntryService _service = null!;

        [SetUp]
        public void SetUp()
        {
            var appCaches = new AppCaches(new ObjectCacheAppCache(), NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));
            _refresher = new SchemaEntryCacheRefresher(appCaches, Mock.Of<IEventAggregator>(), Mock.Of<ICacheRefresherNotificationFactory>());

            // Delivers the refresh to this server, as the real messenger does alongside queueing it for the others.
            _messenger = new Mock<IServerMessenger>();
            _messenger.Setup(it => it.QueueRefreshAll(It.IsAny<ICacheRefresher>()))
                .Callback<ICacheRefresher>(refresher => refresher.RefreshAll());

            _repository = new Mock<ISchemaEntryRepository>();
            _service = new SchemaEntryService(
                _repository.Object,
                appCaches,
                new DistributedCache(_messenger.Object, new CacheRefresherCollection(() => [_refresher])));
        }

        [Test]
        public void GetById_CachesAnEntry()
        {
            var entry = Entry();
            _repository.Setup(it => it.GetById(entry.Id)).Returns(entry);

            Assert.That(_service.GetById(entry.Id), Is.SameAs(entry));
            Assert.That(_service.GetById(entry.Id), Is.SameAs(entry));

            _repository.Verify(it => it.GetById(entry.Id), Times.Once);
        }

        [Test]
        public void GetById_CachesAMissingEntry()
        {
            var id = Guid.NewGuid();

            Assert.That(_service.GetById(id), Is.Null);
            Assert.That(_service.GetByIds([id]), Is.Empty);

            _repository.Verify(it => it.GetById(id), Times.Once);
        }

        [Test]
        public void Add_ClearsACachedMissOnEveryServer()
        {
            var entry = Entry();
            _service.GetById(entry.Id);
            _repository.Setup(it => it.Add(entry)).Returns(entry);
            _repository.Setup(it => it.GetById(entry.Id)).Returns(entry);

            _service.Add(entry);

            _messenger.Verify(it => it.QueueRefreshAll(_refresher), Times.Once);
            Assert.That(_service.GetById(entry.Id), Is.SameAs(entry));
        }

        [Test]
        public void Update_ClearsTheEntryAndItsOwnerOnEveryServer()
        {
            var entry = Entry();
            _repository.Setup(it => it.GetById(entry.Id)).Returns(entry);
            _repository.Setup(it => it.GetAll(OwnerType, entry.OwnerKey)).Returns([entry]);
            _service.GetById(entry.Id);
            _service.GetAll(OwnerType, entry.OwnerKey);

            _service.Update(entry);

            _messenger.Verify(it => it.QueueRefreshAll(_refresher), Times.Once);
            _service.GetById(entry.Id);
            _service.GetAll(OwnerType, entry.OwnerKey);
            _repository.Verify(it => it.GetById(entry.Id), Times.Exactly(2));
            _repository.Verify(it => it.GetAll(OwnerType, entry.OwnerKey), Times.Exactly(2));
        }

        [Test]
        public void Delete_ClearsTheEntryOnEveryServer()
        {
            var entry = Entry();
            _repository.Setup(it => it.GetById(entry.Id)).Returns(entry);
            _service.GetById(entry.Id);

            _service.Delete(entry.Id);

            _repository.Verify(it => it.Delete(entry.Id), Times.Once);
            _messenger.Verify(it => it.QueueRefreshAll(_refresher), Times.Once);
            _repository.Setup(it => it.GetById(entry.Id)).Returns((SchemaEntryDto)null!);
            Assert.That(_service.GetById(entry.Id), Is.Null);
        }

        private static SchemaEntryDto Entry() => new()
        {
            Id = Guid.NewGuid(),
            OwnerType = OwnerType,
            OwnerKey = Guid.NewGuid(),
            SchemaAlias = "Organization",
        };
    }
}
