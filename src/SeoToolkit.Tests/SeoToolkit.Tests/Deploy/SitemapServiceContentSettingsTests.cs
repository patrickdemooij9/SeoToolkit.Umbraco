using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SitemapServiceContentSettingsTests
    {
        private Mock<ISitemapContentRepository> _contentRepository = null!;
        private Mock<IEventAggregator> _eventAggregator = null!;
        private SitemapService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _contentRepository = new Mock<ISitemapContentRepository>();
            _eventAggregator = new Mock<IEventAggregator>();
            _service = new SitemapService(
                Mock.Of<IContentTypeService>(),
                Mock.Of<ISitemapPageTypeRepository>(),
                _contentRepository.Object,
                _eventAggregator.Object);
        }

        [Test]
        public void SetContentSettings_NonDefault_SetsAndPublishesChange()
        {
            var nodeKey = Guid.NewGuid();
            _service.SetContentSettings(new SitemapContentSettings { NodeKey = nodeKey, ExcludeFromSitemap = true });

            _contentRepository.Verify(r => r.Set(It.Is<SitemapContentSettings>(s => s.NodeKey == nodeKey)), Times.Once);
            _eventAggregator.Verify(e => e.Publish(
                It.Is<SitemapContentChangedNotification>(n => n.NodeKey == nodeKey)), Times.Once);
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
        }
    }
}
