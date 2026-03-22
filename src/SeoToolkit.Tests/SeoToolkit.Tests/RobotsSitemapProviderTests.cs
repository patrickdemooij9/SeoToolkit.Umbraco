using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService;
using SeoToolkit.Umbraco.Core.Connectors;
using SeoToolkit.Umbraco.Core.SeoSettings;
using System;
using System.Linq;
using System.Web;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Web.Common;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class RobotsSitemapProviderTests
    {
        [Test]
        public void GetSitemapUrls_NoDomains_ReturnsSingleSitemap()
        {
            // Arrange
            var (provider, mocks) = CreateProviderWithMocks();
            var request = CreateHttpRequest("https://example.com");

            mocks.DomainCache.Setup(it => it.GetAll(false))
                .Returns(Array.Empty<Domain>());

            // Act
            var urls = provider.GetSitemapUrls(request).ToArray();

            // Assert
            Assert.That(urls.Length, Is.EqualTo(1));
            Assert.That(urls[0], Is.EqualTo("https://example.com/sitemap.xml"));
        }

        [Test]
        public void GetSitemapUrls_SingleDomain_ReturnsDomainSitemap()
        {
            // Arrange
            var (provider, mocks) = CreateProviderWithMocks();
            var request = CreateHttpRequest("https://example.com");

            var domain = new Domain(1, "https://mydomain.com", 0, "en-US", true, -1);
            mocks.DomainCache.Setup(it => it.GetAll(false))
                .Returns(new[] { domain });

            // Act
            var urls = provider.GetSitemapUrls(request).ToArray();

            // Assert
            Assert.That(urls.Length, Is.EqualTo(1));
            Assert.That(urls[0], Is.EqualTo("https://mydomain.com/sitemap.xml"));
        }

        [Test]
        public void GetSitemapUrls_DomainStartsWithSlash_UsesRequestAuthority()
        {
            // Arrange
            var (provider, mocks) = CreateProviderWithMocks();
            var request = CreateHttpRequest("https://example.com");

            var domain = new Domain(1, "/en", 0, "en-US", true, -1);
            mocks.DomainCache.Setup(it => it.GetAll(false))
                .Returns(new[] { domain });

            // Act
            var urls = provider.GetSitemapUrls(request).ToArray();

            // Assert
            Assert.That(urls.Length, Is.EqualTo(1));
            Assert.That(urls[0], Is.EqualTo("https://example.com/en/sitemap.xml"));
        }

        [Test]
        public void GetSitemapUrls_DomainWithNoScheme_UsesRequestScheme()
        {
            // Arrange
            var (provider, mocks) = CreateProviderWithMocks();
            var request = CreateHttpRequest("https://example.com");

            var domain = new Domain(1, "example.com", 0, "en-US", true, -1);
            mocks.DomainCache.Setup(it => it.GetAll(false))
                .Returns(new[] { domain });

            // Act
            var urls = provider.GetSitemapUrls(request).ToArray();

            // Assert
            Assert.That(urls.Length, Is.EqualTo(1));
            Assert.That(urls[0], Is.EqualTo("https://example.com/sitemap.xml"));
        }

        [Test]
        public void GetSitemapUrls_MultipleDomains_ReturnsAllSitemaps()
        {
            // Arrange
            var (provider, mocks) = CreateProviderWithMocks();
            var request = CreateHttpRequest("https://example.com");

            var domains = new[]
            {
                new Domain(1, "https://domain1.com", 0, "en-US", true, -1),
                new Domain(2, "https://domain2.com", 0, "en-US", true, -1),
                new Domain(3, "/en", 0, "en-US", true, -1)
            };
            
            mocks.DomainCache.Setup(it => it.GetAll(false))
                .Returns(domains);

            // Act
            var urls = provider.GetSitemapUrls(request).ToArray();

            // Assert
            Assert.That(urls.Length, Is.EqualTo(3));
            Assert.That(urls[0], Is.EqualTo("https://domain1.com/sitemap.xml"));
            Assert.That(urls[1], Is.EqualTo("https://domain2.com/sitemap.xml"));
            Assert.That(urls[2], Is.EqualTo("https://example.com/en/sitemap.xml"));
        }

        [Test]
        public void GetSitemapUrls_WithSeoDomain_FiltersUrls()
        {
            // Arrange
            var (provider, mocks) = CreateProviderWithMocks();
            var request = CreateHttpRequest("https://example.com");

            var domains = new[]
            {
                new Domain(1, "https://domain1.com", 0, "en-US", true, -1),
                new Domain(2, "https://domain2.com", 0, "en-US", true, -1),
                new Domain(3, "https://domain3.com", 0, "en-US", true, -1)
            };
            
            mocks.DomainCache.Setup(it => it.GetAll(false))
                .Returns(domains);

            mocks.SeoDomainResolver.Setup(it => it.ResolveDomain())
                .Returns(new SeoDomainCollection
                { 
                    Name = "Test Domain",
                    DomainIds = new List<int> { 1, 3 } 
                });            // Act
            var urls = provider.GetSitemapUrls(request).ToArray();

            // Assert
            Assert.That(urls.Length, Is.EqualTo(2));
            Assert.That(urls[0], Is.EqualTo("https://domain1.com/sitemap.xml"));
            Assert.That(urls[1], Is.EqualTo("https://domain3.com/sitemap.xml"));
        }

        private (RobotsSitemapProvider Provider, (
            Mock<IUmbracoContextFactory> UmbracoFactory,
            Mock<IUmbracoContext> UmbracoContext,
            Mock<IDomainCache> DomainCache,
            Mock<ISeoDomainResolver> SeoDomainResolver)) CreateProviderWithMocks()
        {
            var umbracoFactory = new Mock<IUmbracoContextFactory>();
            var umbracoContext = new Mock<IUmbracoContext>();
            var domainCache = new Mock<IDomainCache>();
            var seoDomainResolver = new Mock<ISeoDomainResolver>();
            var keyValueService = new Mock<ISeoKeyValueService>();

            umbracoContext.Setup(it => it.Domains).Returns(domainCache.Object);
            umbracoFactory.Setup(it => it.EnsureUmbracoContext())
                .Returns(new UmbracoContextReference(umbracoContext.Object, true, Mock.Of<IUmbracoContextAccessor>()));
            keyValueService.Setup(it => it.GetValue(AutomaticSitemapInRobotsTxtSeoSetting.SettingKey)).Returns("true");

            var provider = new RobotsSitemapProvider(umbracoFactory.Object, seoDomainResolver.Object, keyValueService.Object);

            return (provider, (umbracoFactory, umbracoContext, domainCache, seoDomainResolver));
        }

        private HttpRequest CreateHttpRequest(string url)
        {
            var uri = new Uri(url);
            var request = new Mock<HttpRequest>();
            
            request.Setup(x => x.Scheme).Returns(uri.Scheme);
            request.Setup(x => x.Host).Returns(new HostString(uri.Host));
            request.Setup(x => x.PathBase).Returns(PathString.Empty);
            
            var context = new Mock<HttpContext>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.HttpContext).Returns(context.Object);

            return request.Object;
        }
    }
}