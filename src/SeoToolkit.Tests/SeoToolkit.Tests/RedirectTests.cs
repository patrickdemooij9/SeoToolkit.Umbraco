using Microsoft.Extensions.Options;
using Moq;
using SeoToolkit.Umbraco.Redirects.Core.Caching;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Services;
using System.Net;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class RedirectTests
    {
        [Test]
        public void TestRegexAllSitesRedirect()
        {
            // Arrange
            var redirectId = 1;
            var redirectKey = Guid.NewGuid();
            var redirectRepository = new Mock<IRedirectsRepository>();
            redirectRepository.Setup(it => it.GetAllRegexRedirects()).Returns(() => new Redirect[]
            {
                new Redirect { Domain = null, IsRegex = true, Id = redirectId, Key = redirectKey, OldUrl = "^/test" }
            });
            var bloomFilterMock = new Mock<IRedirectsBloomFilter>();
            bloomFilterMock.Setup(it => it.Contains(It.IsAny<string>())).Returns(true);

            var umbracoContextFactory = GetContextFactoryWithDomain();
            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilterMock.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, new Mock<IOptionsMonitor<RequestHandlerSettings>>().Object);

            // Act
            var redirect = redirectService.GetByUrl(new Uri("https://test.nl/test123"));

            // Assert
            Assert.IsNotNull(redirect);
        }

        [TestCase("hello-world", true)]
        [TestCase("/hello-world", true)]
        [TestCase("https://sw-unlimited-db.com", false)]
        public void TestEnsureNewRedirectStartsWithSlash(string url, bool shouldHaveSlash)
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();

            var umbracoContextFactory = GetContextFactoryWithDomain();

            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(requestHandlerSettings);

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            // Act
            var redirect = new Redirect
            {
                Id = 1,
                Key = Guid.NewGuid(),
                IsEnabled = true,
                IsRegex = false,
                OldUrl = "test",
                NewUrl = url,
                RedirectCode = (int)HttpStatusCode.MovedPermanently
            };
            redirectService.Save(redirect);

            // Assert
            redirectRepository.Verify(it => it.Save(It.Is<Redirect>(r => r.NewUrl.StartsWith("/") == shouldHaveSlash)), Times.Once);
        }

        [TestCase("/hello-world", "/hello-world/")]
        [TestCase("https://sw-unlimited-db.com", "https://sw-unlimited-db.com")]
        [TestCase("/test?hello=1", "/test/?hello=1")]
        public void TestEnsureTrailingSlash(string url, string expectedUrl)
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();

            var umbracoContextFactory = GetContextFactoryWithDomain();

            var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = true };
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(requestHandlerSettings);

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            // Act
            var redirect = new Redirect
            {
                Id = 1,
                Key = Guid.NewGuid(),
                IsEnabled = true,
                IsRegex = false,
                OldUrl = "/test",
                NewUrl = url,
                RedirectCode = (int)HttpStatusCode.MovedPermanently
            };
            redirectService.Save(redirect);

            // Assert
            redirectRepository.Verify(it => it.Save(It.Is<Redirect>(r => r.NewUrl == expectedUrl)), Times.Once);
        }

        [Test]
        public void Save_Throws_WhenRedirectIsNull()
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();
            var umbracoContextFactory = GetContextFactoryWithDomain();
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(new RequestHandlerSettings());

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => redirectService.Save(null));
        }

        [Test]
        public void Save_Throws_WhenRedirectCodeIsNotSupported()
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();
            var umbracoContextFactory = GetContextFactoryWithDomain();
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(new RequestHandlerSettings());

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            var redirect = new Redirect { Id = 1, Key = Guid.NewGuid(), IsEnabled = true, IsRegex = false, OldUrl = "/old", NewUrl = "/new", RedirectCode = 999 };

            // Act / Assert
            Assert.Throws<ArgumentException>(() => redirectService.Save(redirect));
        }

        [Test]
        public void Save_Throws_WhenExistingGlobalRedirectFound()
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();
            var umbracoContextFactory = GetContextFactoryWithDomain();
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(new RequestHandlerSettings());

            var existing = new Redirect { Id = 2, Key = Guid.NewGuid(), Domain = null, CustomDomain = null, OldUrl = "/old" };
            redirectRepository.Setup(it => it.GetByUrls(It.IsAny<string[]>())).Returns(new Redirect[] { existing });

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            var redirect = new Redirect { Id = 1, IsEnabled = true, IsRegex = false, OldUrl = "old", NewUrl = "/new", RedirectCode = (int)HttpStatusCode.MovedPermanently };

            // Act / Assert
            Assert.Throws<ArgumentException>(() => redirectService.Save(redirect));
        }

        [Test]
        public void Delete_RemovesRedirect_WhenFound()
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();
            var umbracoContextFactory = GetContextFactoryWithDomain();
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(new RequestHandlerSettings());

            var redirect = new Redirect { Id = 3, Key = Guid.NewGuid(), OldUrl = "/old" };
            redirectRepository.Setup(it => it.Get(3)).Returns(redirect);

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            // Act
            redirectService.Delete(new int[] { 3 });

            // Assert
            redirectRepository.Verify(it => it.Delete(It.Is<Redirect>(r => r.Id == 3)), Times.Once);
        }

        [Test]
        public void UpdateRedirectCodes_CallsRepository()
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();
            var umbracoContextFactory = GetContextFactoryWithDomain();
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(new RequestHandlerSettings());

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            // Act
            redirectService.UpdateRedirectCodes(new int[] { 1, 2 }, (int)HttpStatusCode.MovedPermanently);

            // Assert
            redirectRepository.Verify(it => it.UpdateRedirectCodes(It.Is<int[]>(arr => arr.Length == 2 && arr[0] == 1 && arr[1] == 2), (int)HttpStatusCode.MovedPermanently), Times.Once);
        }

        [Test]
        public void GetByUrl_ReturnsExactGlobalMatch()
        {
            // Arrange
            var redirectRepository = new Mock<IRedirectsRepository>();
            var bloomFilter = new Mock<IRedirectsBloomFilter>();
            var umbracoContextFactory = GetContextFactoryWithDomain();
            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(it => it.CurrentValue).Returns(new RequestHandlerSettings());

            var uri = new Uri("https://example.com/test");
            var existing = new Redirect { Id = 4, Key = Guid.NewGuid(), OldUrl = "/test", NewUrl = "/new" };
            redirectRepository.Setup(it => it.GetByUrls(It.IsAny<string[]>())).Returns(new Redirect[] { existing });
            bloomFilter.Setup(it => it.Contains(It.IsAny<string>())).Returns(true);

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IEventAggregator>().Object, optionsMonitor.Object);

            // Act
            var result = redirectService.GetByUrl(uri);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(existing.Id, result.Redirect.Id);
            Assert.AreEqual(existing.Key, result.Redirect.Key);
        }

        private IUmbracoContextFactory GetContextFactoryWithDomain()
        {
            var umbracoContextFactory = new Mock<IUmbracoContextFactory>();
            var umbracoContext = new Mock<IUmbracoContext>();
            var domainCache = new Mock<IDomainCache>();

            domainCache.Setup(it => it.GetAll(false)).Returns(Enumerable.Empty<Domain>());
            umbracoContext.Setup(it => it.Domains).Returns(() => domainCache.Object);
            umbracoContextFactory.Setup(it => it.EnsureUmbracoContext()).Returns(() => new UmbracoContextReference(umbracoContext.Object, true, Mock.Of<IUmbracoContextAccessor>()));

            return umbracoContextFactory.Object;
        }
    }
}
