using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator;
using SeoToolkit.Umbraco.Sitemap.Core.Config;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Middleware;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SitemapMiddlewareTests
    {
        private Mock<RequestDelegate> _mockNext = null!;
        private Mock<IUmbracoContextFactory> _mockUmbracoContextFactory = null!;
        private Mock<ISettingsService<SitemapConfig>> _mockSettingsService = null!;
        private Mock<ILogger<SitemapMiddleware>> _mockLogger = null!;
        private Mock<ISitemapGenerator> _mockSitemapGenerator = null!;
        private Mock<ISitemapIndexGenerator> _mockSitemapIndexGenerator = null!;
        private Mock<SeoToolkit.Umbraco.Common.Core.Helpers.ISeoDomainResolver> _mockSeoDomainResolver = null!;
        private SitemapMiddleware _middleware = null!;
        private DefaultHttpContext _httpContext = null!;

        [SetUp]
        public void SetUp()
        {
            _mockNext = new Mock<RequestDelegate>();
            _mockUmbracoContextFactory = new Mock<IUmbracoContextFactory>();
            _mockSettingsService = new Mock<ISettingsService<SitemapConfig>>();
            _mockLogger = new Mock<ILogger<SitemapMiddleware>>();
            _mockSitemapGenerator = new Mock<ISitemapGenerator>();
            _mockSitemapIndexGenerator = new Mock<ISitemapIndexGenerator>();
            _mockSeoDomainResolver = new Mock<SeoToolkit.Umbraco.Common.Core.Helpers.ISeoDomainResolver>();

            _middleware = new SitemapMiddleware(
                _mockNext.Object,
                _mockUmbracoContextFactory.Object,
                _mockSettingsService.Object);

            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        [Test]
        public async Task Invoke_WithNonSitemapRequest_PassesToNextMiddleware()
        {
            // Arrange
            _httpContext.Request.Path = "/some-page";

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            _mockNext.Verify(n => n.Invoke(_httpContext), Times.Once);
        }

        [Test]
        public async Task Invoke_WithSitemapXmlRequest_GeneratesSitemap()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { ReturnContentType = "application/xml", StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/xml"));
            _mockSitemapGenerator.Verify(g => g.Generate(It.IsAny<SitemapGeneratorOptions>()), Times.Once);
            _mockNext.Verify(n => n.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        [Test]
        public async Task Invoke_WithNoDomains_CallsSitemapGeneratorWithNullStartingNode()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { ReturnContentType = "application/xml" };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            _mockSitemapGenerator.Verify(
                g => g.Generate(It.Is<SitemapGeneratorOptions>(o => o.StartingNode == null)),
                Times.Once);
        }

        [Test]
        public async Task Invoke_WithStructureModeOnlyRoot_CallsSitemapGeneratorWithNullStartingNode()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { StructureMode = StructureMode.OnlyRoot, ReturnContentType = "application/xml" };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            _mockSitemapGenerator.Verify(
                g => g.Generate(It.Is<SitemapGeneratorOptions>(o => o.StartingNode == null)),
                Times.Once);
        }

        [Test]
        public async Task Invoke_WithSitemapRequest_SetsResponseStatusCode200()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { ReturnContentType = "application/xml", StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Invoke_WithSitemapRequest_SetsCustomContentType()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var customContentType = "text/xml; charset=utf-8";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { ReturnContentType = customContentType, StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo(customContentType));
        }

        [Test]
        public async Task Invoke_WithSitemapRequest_WritesXmlContentToResponse()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
            var sitemapXml = new XDocument(
                new XElement(ns + "urlset",
                    new XElement(ns + "url",
                        new XElement(ns + "loc", "https://example.com/"),
                        new XElement(ns + "lastmod", "2024-01-01"))));
            var settings = new SitemapConfig { ReturnContentType = "application/xml", StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(_httpContext.Response.Body, Encoding.UTF8))
            {
                var responseContent = reader.ReadToEnd();
                Assert.That(responseContent, Does.Contain("urlset"));
                Assert.That(responseContent, Does.Contain("https://example.com/"));
            }
        }

        [Test]
        public async Task Invoke_WithCaseInsensitiveSitemapPath_GeneratesSitemap()
        {
            // Arrange
            _httpContext.Request.Path = "/Sitemap.XML";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { ReturnContentType = "application/xml", StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
            _mockSitemapGenerator.Verify(g => g.Generate(It.IsAny<SitemapGeneratorOptions>()), Times.Once);
        }

        [Test]
        public async Task Invoke_WithValidSitemapXml_ResponseBodyIsNotEmpty()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var sitemapXml = new XDocument(
                new XElement("urlset",
                    new XElement("url",
                        new XElement("loc", "https://example.com/page1"))));
            var settings = new SitemapConfig { ReturnContentType = "application/xml", StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyLength = _httpContext.Response.Body.Length;
            Assert.That(bodyLength, Is.GreaterThan(0), "Response body should not be empty");
        }

        [Test]
        public async Task Invoke_WithSitemapRequest_CallsSettingsService()
        {
            // Arrange
            _httpContext.Request.Path = "/sitemap.xml";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { ReturnContentType = "application/xml", StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            _mockSettingsService.Verify(s => s.GetSettings(), Times.Once);
        }

        [Test]
        public async Task Invoke_WithNonSitemapPath_DoesNotCallSettingsService()
        {
            // Arrange
            _httpContext.Request.Path = "/page";

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            _mockSettingsService.Verify(s => s.GetSettings(), Times.Never);
        }

        [Test]
        public async Task Invoke_WithSitemapXmlInUrlPath_GeneratesSitemap()
        {
            // Arrange
            _httpContext.Request.Path = "/some/path/to/sitemap.xml";
            var sitemapXml = new XDocument(new XElement("urlset"));
            var settings = new SitemapConfig { ReturnContentType = "application/xml", StructureMode = StructureMode.OnlyRoot };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapGenerator.Setup(g => g.Generate(It.IsAny<SitemapGeneratorOptions>())).Returns(sitemapXml);
            SetupBasicUmbracoContext();

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object, _mockSeoDomainResolver.Object);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
            _mockSitemapGenerator.Verify(g => g.Generate(It.IsAny<SitemapGeneratorOptions>()), Times.Once);
        }

        [Test]
        public async Task Invoke_WithConfiguredIndexPath_AndMatchingRequest_GeneratesSitemapIndex()
        {
            // Arrange - a dedicated index path is configured and the request targets it, while multiple domains exist.
            _httpContext.Request.Path = "/sitemap-index/sitemap.xml";
            var settings = new SitemapConfig
            {
                ReturnContentType = "application/xml",
                StructureMode = StructureMode.MultiRoot,
                SitemapIndexPath = "sitemap-index"
            };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapIndexGenerator.Setup(g => g.Generate()).Returns(new XDocument(new XElement("sitemapindex")));
            SetupUmbracoContextWithDomains(2);

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object);

            // Assert - the index is served here and no per-domain sitemap is generated.
            _mockSitemapIndexGenerator.Verify(g => g.Generate(), Times.Once);
            _mockSitemapGenerator.Verify(g => g.Generate(It.IsAny<SitemapGeneratorOptions>()), Times.Never);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Invoke_WithConfiguredIndexFilename_AndMatchingRequest_GeneratesSitemapIndex()
        {
            // Arrange - the index path is configured as a full ".xml" filename, served directly off the root.
            _httpContext.Request.Path = "/sitemap-index.xml";
            var settings = new SitemapConfig
            {
                ReturnContentType = "application/xml",
                StructureMode = StructureMode.MultiRoot,
                SitemapIndexPath = "sitemap-index.xml"
            };

            _mockSettingsService.Setup(s => s.GetSettings()).Returns(settings);
            _mockSitemapIndexGenerator.Setup(g => g.Generate()).Returns(new XDocument(new XElement("sitemapindex")));
            SetupUmbracoContextWithDomains(2);

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object);

            // Assert
            _mockSitemapIndexGenerator.Verify(g => g.Generate(), Times.Once);
            _mockSitemapGenerator.Verify(g => g.Generate(It.IsAny<SitemapGeneratorOptions>()), Times.Never);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Invoke_WithNonXmlPath_DoesNotCallSettingsService()
        {
            // Arrange - the cheap pre-guard should reject non-.xml requests before touching settings.
            _httpContext.Request.Path = "/sitemap-index";

            // Act
            await _middleware.Invoke(_httpContext, _mockSitemapGenerator.Object, _mockSitemapIndexGenerator.Object);

            // Assert
            _mockSettingsService.Verify(s => s.GetSettings(), Times.Never);
            _mockNext.Verify(n => n.Invoke(_httpContext), Times.Once);
        }

        private void SetupUmbracoContextWithDomains(int domainCount)
        {
            var mockUmbracoContext = new Mock<IUmbracoContext>();
            var mockAccessor = new Mock<global::Umbraco.Cms.Core.Web.IUmbracoContextAccessor>();

            var domains = Enumerable.Range(1, domainCount)
                .Select(i => new Domain(i, $"/lang{i}", contentId: i, culture: $"lang{i}", isWildcard: false, sortOrder: i))
                .ToArray();

            var mockDomainCache = new Mock<IDomainCache>();
            mockDomainCache.Setup(d => d.GetAll(It.IsAny<bool>())).Returns(domains);
            mockDomainCache.Setup(d => d.DefaultCulture).Returns("en");
            mockUmbracoContext.Setup(c => c.Domains).Returns(mockDomainCache.Object);

            var mockContentCache = new Mock<IPublishedContentCache>();
            mockUmbracoContext.Setup(c => c.Content).Returns(mockContentCache.Object);

            var ctxRef = new UmbracoContextReference(mockUmbracoContext.Object, true, mockAccessor.Object);
            _mockUmbracoContextFactory.Setup(f => f.EnsureUmbracoContext())
                .Returns(ctxRef);
        }

        private void SetupBasicUmbracoContext()
        {
            var mockUmbracoContext = new Mock<IUmbracoContext>();
            var mockAccessor = new Mock<global::Umbraco.Cms.Core.Web.IUmbracoContextAccessor>();

            // Setup Domains cache (minimal - just needs to return empty enumerable)
            var mockDomainCache = new Mock<IDomainCache>();
            mockDomainCache.Setup(d => d.GetAll(It.IsAny<bool>())).Returns(Enumerable.Empty<Domain>());
            mockDomainCache.Setup(d => d.DefaultCulture).Returns("en");
            mockUmbracoContext.Setup(c => c.Domains).Returns(mockDomainCache.Object);

            // Setup Content cache (minimal)
            var mockContentCache = new Mock<IPublishedContentCache>();
            mockUmbracoContext.Setup(c => c.Content).Returns(mockContentCache.Object);

            // Create UmbracoContextReference with mocked context
            var ctxRef = new UmbracoContextReference(mockUmbracoContext.Object, true, mockAccessor.Object);
            _mockUmbracoContextFactory.Setup(f => f.EnsureUmbracoContext())
                .Returns(ctxRef);
        }
    }
}
