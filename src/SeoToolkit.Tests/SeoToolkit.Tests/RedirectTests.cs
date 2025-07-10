using Microsoft.Extensions.Options;
using Moq;
using SeoToolkit.Umbraco.Redirects.Core.Caching;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Services;
using System.Net;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
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
            var redirectRepository = new Mock<IRedirectsRepository>();
            redirectRepository.Setup(it => it.GetAllRegexRedirects()).Returns(() => new Redirect[]
            {
                new Redirect { Domain = null, IsRegex = true, Id = redirectId, OldUrl = "^/test" }
            });
            var bloomFilter = new Mock<IRedirectsBloomFilter>();
            bloomFilter.Setup(it => it.Contains(It.IsAny<string>())).Returns(true);

            var umbracoContextFactory = GetContextFactoryWithDomain();
            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, new Mock<IOptionsMonitor<RequestHandlerSettings>>().Object);

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

            var redirectService = new RedirectsService(redirectRepository.Object, bloomFilter.Object, umbracoContextFactory, optionsMonitor.Object);

            // Act
            var redirect = new Redirect
            {
                Id = 1,
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

        private IUmbracoContextFactory GetContextFactoryWithDomain()
        {
            var umbracoContextFactory = new Mock<IUmbracoContextFactory>();
            var umbracoContext = new Mock<IUmbracoContext>();
            var domainCache = new Mock<IDomainCache>();

            domainCache.Setup(it => it.GetAll(false)).Returns(Enumerable.Empty<Domain>);
            umbracoContext.Setup(it => it.Domains).Returns(() => domainCache.Object);
            umbracoContextFactory.Setup(it => it.EnsureUmbracoContext()).Returns(() => new UmbracoContextReference(umbracoContext.Object, true, Mock.Of<IUmbracoContextAccessor>()));

            return umbracoContextFactory.Object;
        }
    }
}
