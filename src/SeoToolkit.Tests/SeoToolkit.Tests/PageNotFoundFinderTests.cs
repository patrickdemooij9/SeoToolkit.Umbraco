using Moq;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.NotFound.Core.ContentFinders;
using SeoToolkit.Umbraco.NotFound.Core.Notifications;
using SeoToolkit.Umbraco.NotFound.Core.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class PageNotFoundFinderTests
    {
        private Mock<IPageNotFoundService> _pageNotFoundServiceMock;
        private Mock<ISeoDomainResolver> _seoDomainResolverMock;
        private Mock<IUmbracoContextAccessor> _umbracoContextAccessorMock;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private PageNotFoundFinder _finder;

        [SetUp]
        public void Setup()
        {
            _pageNotFoundServiceMock = new Mock<IPageNotFoundService>();
            _seoDomainResolverMock = new Mock<ISeoDomainResolver>();
            _umbracoContextAccessorMock = new Mock<IUmbracoContextAccessor>();
            _eventAggregatorMock = new Mock<IEventAggregator>();

            _finder = new PageNotFoundFinder(
                _pageNotFoundServiceMock.Object,
                _seoDomainResolverMock.Object,
                _umbracoContextAccessorMock.Object,
                _eventAggregatorMock.Object
            );
        }

        [Test]
        public async Task TryFindContent_WhenNoPageNotFoundConfigured_ReturnsFalse()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            IUmbracoContext? context = null;
            
            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns((SeoDomainCollection)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns((Guid?)null);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out context)).Returns(false);

            // Act
            var result = await _finder.TryFindContent(request.Object);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TryFindContent_WhenPageNotFoundIsNull_ReturnsFalse()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var domainId = Guid.NewGuid();
            var seoDomain = new SeoDomainCollection { Id = domainId, Name = "Test Domain" };
            IUmbracoContext? context = null;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns(seoDomain);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(domainId)).Returns((Guid?)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns((Guid?)null);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out context)).Returns(false);

            // Act
            var result = await _finder.TryFindContent(request.Object);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TryFindContent_FallsBackToRootWhenDomainSpecificNotFound()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var domainId = Guid.NewGuid();
            var pageNotFoundGuid = Guid.NewGuid();
            var seoDomain = new SeoDomainCollection { Id = domainId, Name = "Test Domain" };

            var pageMock = new Mock<IPublishedContent>();
            pageMock.Setup(x => x.IsPublished()).Returns(true);

            var contextMock = new Mock<IUmbracoContext>();
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contextMock.Setup(x => x.Content).Returns(contentCacheMock.Object);
            contentCacheMock.Setup(x => x.GetById(pageNotFoundGuid)).Returns(pageMock.Object);

            IUmbracoContext? outContext = contextMock.Object;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns(seoDomain);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(domainId)).Returns((Guid?)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns(pageNotFoundGuid);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);
            _eventAggregatorMock.Setup(x => x.PublishAsync(It.IsAny<PageNotFoundNotification>())).Returns(Task.CompletedTask);

            // Act
            var result = await _finder.TryFindContent(request.Object);

            // Assert
            Assert.That(result, Is.True);
            _pageNotFoundServiceMock.Verify(x => x.GetPageNotFound(domainId), Times.Once);
            _pageNotFoundServiceMock.Verify(x => x.GetPageNotFound(null), Times.Once);
            request.Verify(x => x.SetResponseStatus(404), Times.Once);
            request.Verify(x => x.SetPublishedContent(pageMock.Object), Times.Once);
        }

        [Test]
        public async Task TryFindContent_WhenPageNotFound_UsesSpecificDomainPage()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var domainId = Guid.NewGuid();
            var pageNotFoundGuid = Guid.NewGuid();
            var seoDomain = new SeoDomainCollection { Id = domainId, Name = "Test Domain" };

            var pageMock = new Mock<IPublishedContent>();
            pageMock.Setup(x => x.IsPublished()).Returns(true);

            var contextMock = new Mock<IUmbracoContext>();
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contextMock.Setup(x => x.Content).Returns(contentCacheMock.Object);
            contentCacheMock.Setup(x => x.GetById(pageNotFoundGuid)).Returns(pageMock.Object);

            IUmbracoContext? outContext = contextMock.Object;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns(seoDomain);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(domainId)).Returns(pageNotFoundGuid);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);
            _eventAggregatorMock.Setup(x => x.PublishAsync(It.IsAny<PageNotFoundNotification>())).Returns(Task.CompletedTask);

            // Act
            var result = await _finder.TryFindContent(request.Object);

            // Assert
            Assert.That(result, Is.True);
            _pageNotFoundServiceMock.Verify(x => x.GetPageNotFound(domainId), Times.Once);
            _pageNotFoundServiceMock.Verify(x => x.GetPageNotFound(null), Times.Never);
            request.Verify(x => x.SetResponseStatus(404), Times.Once);
            request.Verify(x => x.SetPublishedContent(pageMock.Object), Times.Once);
        }

        [Test]
        public async Task TryFindContent_WhenContentNotFound_ReturnsFalse()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var pageNotFoundGuid = Guid.NewGuid();

            var contextMock = new Mock<IUmbracoContext>();
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contextMock.Setup(x => x.Content).Returns(contentCacheMock.Object);
            contentCacheMock.Setup(x => x.GetById(pageNotFoundGuid)).Returns((IPublishedContent)null);

            IUmbracoContext? outContext = contextMock.Object;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns((SeoDomainCollection)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns(pageNotFoundGuid);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);
            _eventAggregatorMock.Setup(x => x.PublishAsync(It.IsAny<PageNotFoundNotification>())).Returns(Task.CompletedTask);

            // Act
            var result = await _finder.TryFindContent(request.Object);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TryFindContent_WhenPageNotPublished_ReturnsFalse()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var pageNotFoundGuid = Guid.NewGuid();

            var pageMock = new Mock<IPublishedContent>();
            pageMock.Setup(x => x.IsPublished()).Returns(false);

            var contextMock = new Mock<IUmbracoContext>();
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contextMock.Setup(x => x.Content).Returns(contentCacheMock.Object);
            contentCacheMock.Setup(x => x.GetById(pageNotFoundGuid)).Returns(pageMock.Object);

            IUmbracoContext? outContext = contextMock.Object;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns((SeoDomainCollection)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns(pageNotFoundGuid);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);
            _eventAggregatorMock.Setup(x => x.PublishAsync(It.IsAny<PageNotFoundNotification>())).Returns(Task.CompletedTask);

            // Act
            var result = await _finder.TryFindContent(request.Object);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TryFindContent_PublishesPageNotFoundNotification()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var pageNotFoundGuid = Guid.NewGuid();

            var pageMock = new Mock<IPublishedContent>();
            pageMock.Setup(x => x.IsPublished()).Returns(true);

            var contextMock = new Mock<IUmbracoContext>();
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contextMock.Setup(x => x.Content).Returns(contentCacheMock.Object);
            contentCacheMock.Setup(x => x.GetById(pageNotFoundGuid)).Returns(pageMock.Object);

            IUmbracoContext? outContext = contextMock.Object;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns((SeoDomainCollection)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns(pageNotFoundGuid);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);
            _eventAggregatorMock.Setup(x => x.PublishAsync(It.IsAny<PageNotFoundNotification>())).Returns(Task.CompletedTask);

            // Act
            await _finder.TryFindContent(request.Object);

            // Assert
            _eventAggregatorMock.Verify(x => x.PublishAsync(It.Is<PageNotFoundNotification>(n => n.Page == pageMock.Object)), Times.Once);
        }

        [Test]
        public async Task TryFindContent_SetsResponseStatusTo404()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var pageNotFoundGuid = Guid.NewGuid();

            var pageMock = new Mock<IPublishedContent>();
            pageMock.Setup(x => x.IsPublished()).Returns(true);

            var contextMock = new Mock<IUmbracoContext>();
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contextMock.Setup(x => x.Content).Returns(contentCacheMock.Object);
            contentCacheMock.Setup(x => x.GetById(pageNotFoundGuid)).Returns(pageMock.Object);

            IUmbracoContext? outContext = contextMock.Object;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns((SeoDomainCollection)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns(pageNotFoundGuid);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out outContext)).Returns(true);
            _eventAggregatorMock.Setup(x => x.PublishAsync(It.IsAny<PageNotFoundNotification>())).Returns(Task.CompletedTask);

            // Act
            await _finder.TryFindContent(request.Object);

            // Assert
            request.Verify(x => x.SetResponseStatus(404), Times.Once);
        }

        [Test]
        public async Task TryFindContent_WhenUmbracoContextNotAvailable_HandlesGracefully()
        {
            // Arrange
            var request = new Mock<IPublishedRequestBuilder>();
            var pageNotFoundGuid = Guid.NewGuid();
            IUmbracoContext? context = null;

            _seoDomainResolverMock.Setup(x => x.ResolveDomain()).Returns((SeoDomainCollection)null);
            _pageNotFoundServiceMock.Setup(x => x.GetPageNotFound(null)).Returns(pageNotFoundGuid);
            _umbracoContextAccessorMock.Setup(x => x.TryGetUmbracoContext(out context)).Returns(false);
            _eventAggregatorMock.Setup(x => x.PublishAsync(It.IsAny<PageNotFoundNotification>())).Returns(Task.CompletedTask);

            // Act
            var result = await _finder.TryFindContent(request.Object);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
