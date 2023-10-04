using Moq;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Services;
using Umbraco.Cms.Core;
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

            var umbracoContextFactory = GetContextFactoryWithDomain();
            var redirectService = new RedirectsService(redirectRepository.Object, umbracoContextFactory);

            // Act
            var redirect = redirectService.GetByUrl(new Uri("https://test.nl/test123"));

            // Assert
            Assert.IsNotNull(redirect);
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
