using Microsoft.AspNetCore.Mvc;
using Moq;
using SeoToolkit.Umbraco.Sitemap.Core.Controllers;
using SeoToolkit.Umbraco.Sitemap.Core.Models.PostModels;
using SeoToolkit.Umbraco.Sitemap.Core.Models.ViewModels;
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
        public void GetContentSettings_WhenContentDoesNotExist_ReturnsEmptySettingsWithOk()
        {
            var result = _controller.GetContentSettings(Guid.NewGuid());

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = (OkObjectResult)result;
            Assert.That(ok.Value, Is.TypeOf<SitemapContentSettingsViewModel>());

            var model = (SitemapContentSettingsViewModel)ok.Value!;
            Assert.That(model.ExcludeFromSitemap, Is.False);
            Assert.That(model.ChangeFrequency, Is.Null);
            Assert.That(model.Priority, Is.Null);
            Assert.That(model.InheritedChangeFrequency, Is.Null);
            Assert.That(model.InheritedPriority, Is.Null);
        }

        [Test]
        public void SetContentSettings_WhenContentDoesNotExist_ReturnsNotFound()
        {
            var nodeKey = Guid.NewGuid();
            var model = new SitemapContentSettingsPostModel
            {
                NodeKey = nodeKey
            };

            var result = _controller.SetContentSettings(model);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }
    }
}
