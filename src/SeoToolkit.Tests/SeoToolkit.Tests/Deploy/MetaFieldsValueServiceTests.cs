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
        public void ReplaceAllValues_WritesEachCultureAsIs_AndDeletesValuesNotInTheSet()
        {
            var nodeKey = Guid.NewGuid();
            var repository = new Mock<IMetaFieldsValueRepository>();
            repository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "Old title", ["description"] = "Stale" },
                ["nl-NL"] = new() { ["title"] = "Oud" },
            });
            repository.Setup(r => r.Exists(nodeKey, "title", "")).Returns(true);
            var eventAggregator = new Mock<IEventAggregator>();

            var service = CreateService(repository, eventAggregator);
            service.ReplaceAllValues(nodeKey, new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "New" },
                ["da-DK"] = new() { ["title"] = "Hej" },
            });

            // Invariant values stay under "" rather than falling back to the variation context culture.
            repository.Verify(r => r.Update(nodeKey, "title", "", "New"), Times.Once);
            repository.Verify(r => r.Add(nodeKey, "title", "da-DK", "Hej"), Times.Once);
            repository.Verify(r => r.Delete(nodeKey, "description", ""), Times.Once);
            repository.Verify(r => r.Delete(nodeKey, "title", "nl-NL"), Times.Once);
            repository.Verify(r => r.Delete(nodeKey, "title", ""), Times.Never);
            repository.Verify(r => r.Add(It.IsAny<Guid>(), It.IsAny<string>(), "en-US", It.IsAny<object>()), Times.Never);
            eventAggregator.Verify(e => e.Publish(
                It.Is<MetaFieldsValueChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
        }

        [Test]
        public void ReplaceAllValues_WithNoValues_ClearsAllTargetValues()
        {
            var nodeKey = Guid.NewGuid();
            var repository = new Mock<IMetaFieldsValueRepository>();
            repository.Setup(r => r.GetAllValues(nodeKey)).Returns(new Dictionary<string, Dictionary<string, object>>
            {
                [""] = new() { ["title"] = "Old", ["description"] = "Old" },
                ["da-DK"] = new() { ["title"] = "Gammel" },
            });
            var eventAggregator = new Mock<IEventAggregator>();

            var service = CreateService(repository, eventAggregator);
            service.ReplaceAllValues(nodeKey, new Dictionary<string, Dictionary<string, object>>());

            repository.Verify(r => r.Delete(nodeKey, "title", ""), Times.Once);
            repository.Verify(r => r.Delete(nodeKey, "description", ""), Times.Once);
            repository.Verify(r => r.Delete(nodeKey, "title", "da-DK"), Times.Once);
            repository.Verify(r => r.Add(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            eventAggregator.Verify(e => e.Publish(
                It.Is<MetaFieldsValueChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
        }
    }
}
