using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Sitemap.Core.Caching;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SitemapServiceContentSettingsTests
    {
        private Mock<ISitemapContentRepository> _contentRepository = null!;
        private Mock<IContentTypeService> _contentTypeService = null!;
        private Mock<IEventAggregator> _eventAggregator = null!;
        private Mock<IServerMessenger> _messenger = null!;
        private Mock<ICacheRefresher> _sitemapRefresher = null!;
        private SitemapService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _contentRepository = new Mock<ISitemapContentRepository>();
            _contentTypeService = new Mock<IContentTypeService>();
            _eventAggregator = new Mock<IEventAggregator>();

            _messenger = new Mock<IServerMessenger>();
            _sitemapRefresher = new Mock<ICacheRefresher>();
            _sitemapRefresher.SetupGet(r => r.RefresherUniqueId).Returns(SitemapCacheRefresher.CacheGuid);
            var distributedCache = new DistributedCache(_messenger.Object, new CacheRefresherCollection(() => new[] { _sitemapRefresher.Object }));

            _service = new SitemapService(
                _contentTypeService.Object,
                Mock.Of<ISitemapPageTypeRepository>(),
                _contentRepository.Object,
                _eventAggregator.Object,
                distributedCache);
        }

        [Test]
        public void SetContentSettings_NonDefault_SetsAndPublishesChange()
        {
            var nodeKey = Guid.NewGuid();
            _service.SetContentSettings(new SitemapContentSettings { NodeKey = nodeKey, ExcludeFromSitemap = true });

            _contentRepository.Verify(r => r.Set(It.Is<SitemapContentSettings>(s => s.NodeKey == nodeKey)), Times.Once);
            _eventAggregator.Verify(e => e.Publish(
                It.Is<SitemapContentChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
            _messenger.Verify(m => m.QueueRefreshAll(_sitemapRefresher.Object), Times.Once);
        }

        [Test]
        public void SetContentSettings_ResetToDefault_DeletesAndPublishesChange()
        {
            var nodeKey = Guid.NewGuid();
            // All-default settings: the row is deleted rather than stored.
            _service.SetContentSettings(new SitemapContentSettings { NodeKey = nodeKey });

            _contentRepository.Verify(r => r.Delete(nodeKey), Times.Once);
            _contentRepository.Verify(r => r.Set(It.IsAny<SitemapContentSettings>()), Times.Never);
            _eventAggregator.Verify(e => e.Publish(
                It.Is<SitemapContentChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
            _messenger.Verify(m => m.QueueRefreshAll(_sitemapRefresher.Object), Times.Once);
        }

        [Test]
        public void SetPageTypeSettings_ClearsSitemapCacheOnAllServers()
        {
            var contentTypeKey = Guid.NewGuid();
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(Mock.Of<IContentType>(c => c.IsElement == false));

            _service.SetPageTypeSettings(new SitemapPageSettings { ContentTypeGuid = contentTypeKey });

            _messenger.Verify(m => m.QueueRefreshAll(_sitemapRefresher.Object), Times.Once);
        }
    }
}
