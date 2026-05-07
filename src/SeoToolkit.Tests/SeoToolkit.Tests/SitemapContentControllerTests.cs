using Microsoft.AspNetCore.Mvc;
using Moq;
using SeoToolkit.Umbraco.Sitemap.Core.Controllers;
using SeoToolkit.Umbraco.Sitemap.Core.Models.PostModels;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SitemapContentControllerTests
    {
        private Mock<ISitemapService> _sitemapService = null!;
        private Mock<IUmbracoContextFactory> _umbracoContextFactory = null!;
        private SitemapContentController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _sitemapService = new Mock<ISitemapService>();
            _umbracoContextFactory = new Mock<IUmbracoContextFactory>();

            var umbracoContext = new Mock<IUmbracoContext>();
            var contentCache = new Mock<IPublishedContentCache>();
            contentCache.Setup(x => x.GetById(true, It.IsAny<Guid>())).Returns((IPublishedContent)null!);
            umbracoContext.Setup(x => x.Content).Returns(contentCache.Object);

            var contextReference = new UmbracoContextReference(umbracoContext.Object, true, Mock.Of<IUmbracoContextAccessor>());
            _umbracoContextFactory.Setup(x => x.EnsureUmbracoContext()).Returns(contextReference);

            _controller = new SitemapContentController(_sitemapService.Object, _umbracoContextFactory.Object);
        }

        [Test]
        public void GetContentSettings_WhenContentDoesNotExist_ReturnsProblemDetailsNotFound()
        {
            var nodeKey = Guid.NewGuid();

            var result = _controller.GetContentSettings(nodeKey);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFound = (NotFoundObjectResult)result;
            Assert.That(notFound.Value, Is.TypeOf<ProblemDetails>());

            var problemDetails = (ProblemDetails)notFound.Value!;
            Assert.That(problemDetails.Status, Is.EqualTo(404));
            Assert.That(problemDetails.Title, Is.EqualTo("Content not found"));
            Assert.That(problemDetails.Detail, Is.EqualTo($"Cannot find content by key: {nodeKey}"));
        }

        [Test]
        public void SetContentSettings_WhenContentDoesNotExist_ReturnsProblemDetailsNotFound()
        {
            var nodeKey = Guid.NewGuid();
            var model = new SitemapContentSettingsPostModel
            {
                NodeKey = nodeKey
            };

            var result = _controller.SetContentSettings(model);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFound = (NotFoundObjectResult)result;
            Assert.That(notFound.Value, Is.TypeOf<ProblemDetails>());

            var problemDetails = (ProblemDetails)notFound.Value!;
            Assert.That(problemDetails.Status, Is.EqualTo(404));
            Assert.That(problemDetails.Title, Is.EqualTo("Content not found"));
            Assert.That(problemDetails.Detail, Is.EqualTo($"Cannot find content by key: {nodeKey}"));
        }
    }
}
