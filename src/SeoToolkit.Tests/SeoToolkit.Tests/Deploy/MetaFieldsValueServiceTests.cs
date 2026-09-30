using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.MetaFields.Core.Caching;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class MetaFieldsValueServiceTests
    {
        private static MetaFieldsValueService CreateService(
            Mock<IMetaFieldsValueRepository> repository, Mock<IEventAggregator> eventAggregator)
        {
            var refresher = new Mock<ICacheRefresher>();
            refresher.SetupGet(r => r.RefresherUniqueId).Returns(SeoValueCacheRefresher.CacheGuid);
            var distributedCache = new DistributedCache(
                Mock.Of<IServerMessenger>(), new CacheRefresherCollection(() => new[] { refresher.Object }));

            var variationContextAccessor = new Mock<IVariationContextAccessor>();
            variationContextAccessor.SetupGet(v => v.VariationContext).Returns(new VariationContext("en-US"));

            return new MetaFieldsValueService(
                repository.Object, variationContextAccessor.Object, AppCaches.NoCache, distributedCache, eventAggregator.Object);
        }

        [Test]
        public void AddValues_PublishesChangedNotification()
        {
            var nodeKey = Guid.NewGuid();
            var repository = new Mock<IMetaFieldsValueRepository>();
            var eventAggregator = new Mock<IEventAggregator>();

            var service = CreateService(repository, eventAggregator);
            service.AddValues(nodeKey, new Dictionary<string, object> { ["title"] = "Hello" }, "en-US");

            eventAggregator.Verify(e => e.Publish(
                It.Is<MetaFieldsValueChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
        }

        [Test]
        public void Delete_PublishesChangedNotification()
        {
            var nodeKey = Guid.NewGuid();
            var repository = new Mock<IMetaFieldsValueRepository>();
            var eventAggregator = new Mock<IEventAggregator>();

            var service = CreateService(repository, eventAggregator);
            service.Delete(nodeKey, "title", "en-US");

            eventAggregator.Verify(e => e.Publish(
                It.Is<MetaFieldsValueChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
        }

        [Test]
        public void NotifyChanged_PublishesChangedNotification()
        {
            var nodeKey = Guid.NewGuid();
            var repository = new Mock<IMetaFieldsValueRepository>();
            var eventAggregator = new Mock<IEventAggregator>();

            var service = CreateService(repository, eventAggregator);
            service.NotifyChanged(nodeKey);

            eventAggregator.Verify(e => e.Publish(
                It.Is<MetaFieldsValueChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
        }
    }
}
