using Microsoft.AspNetCore.Http;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Redirects.Core.Config.Models;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Middleware;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using System.Net;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class RedirectMiddlewareTests
    {
        private Mock<IRedirectsService> _mockRedirectsService = null!;
        private Mock<ISettingsService<RedirectsConfigModel>> _mockSettingsService = null!;
        private Mock<IRedirectHitTracker> _mockRedirectHitTracker = null!;
        private RedirectMiddleware _middleware = null!;
        private DefaultHttpContext _httpContext = null!;

        [SetUp]
        public void SetUp()
        {
            var runtimeState = new Mock<IRuntimeState>();
            runtimeState.Setup(it => it.Level).Returns(RuntimeLevel.Run);

            var umbracoContextFactory = new Mock<IUmbracoContextFactory>();
            umbracoContextFactory.Setup(it => it.EnsureUmbracoContext())
                .Returns(() => new UmbracoContextReference(Mock.Of<IUmbracoContext>(), true, Mock.Of<IUmbracoContextAccessor>()));

            _mockRedirectsService = new Mock<IRedirectsService>();
            _mockSettingsService = new Mock<ISettingsService<RedirectsConfigModel>>();
            _mockSettingsService.Setup(it => it.GetSettings()).Returns(new RedirectsConfigModel());
            _mockRedirectHitTracker = new Mock<IRedirectHitTracker>();

            _middleware = new RedirectMiddleware(
                Mock.Of<RequestDelegate>(),
                runtimeState.Object,
                _mockRedirectsService.Object,
                umbracoContextFactory.Object,
                _mockSettingsService.Object,
                _mockRedirectHitTracker.Object);

            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Scheme = "https";
            _httpContext.Request.Host = new HostString("example.com");
            _httpContext.Request.Path = "/old";
        }

        [Test]
        public async Task Invoke_WithPermanentRedirect_SetsPublicMaxAgeFromSettings()
        {
            // Arrange
            SetupRedirect(HttpStatusCode.MovedPermanently);
            _mockSettingsService.Setup(it => it.GetSettings()).Returns(new RedirectsConfigModel { PermanentRedirectMaxAgeInSeconds = 3600 });

            // Act
            await _middleware.Invoke(_httpContext);

            // Assert
            Assert.AreEqual(StatusCodes.Status301MovedPermanently, _httpContext.Response.StatusCode);
            Assert.AreEqual("public, max-age=3600", _httpContext.Response.Headers.CacheControl.ToString());
        }

        [Test]
        public async Task Invoke_WithPermanentRedirect_UsesOneDayByDefault()
        {
            // Arrange
            SetupRedirect(HttpStatusCode.MovedPermanently);

            // Act
            await _middleware.Invoke(_httpContext);

            // Assert
            Assert.AreEqual("public, max-age=86400", _httpContext.Response.Headers.CacheControl.ToString());
        }

        [Test]
        public async Task Invoke_WithTemporaryRedirect_SetsNoStore()
        {
            // Arrange
            SetupRedirect(HttpStatusCode.Redirect);

            // Act
            await _middleware.Invoke(_httpContext);

            // Assert
            Assert.AreEqual(StatusCodes.Status302Found, _httpContext.Response.StatusCode);
            Assert.AreEqual("no-store", _httpContext.Response.Headers.CacheControl.ToString());
        }

        [Test]
        public async Task Invoke_WithMatchingRedirect_RegistersHit()
        {
            // Arrange
            var redirect = SetupRedirect(HttpStatusCode.MovedPermanently);

            // Act
            await _middleware.Invoke(_httpContext);

            // Assert
            _mockRedirectHitTracker.Verify(it => it.RegisterHit(redirect.Key), Times.Once);
        }

        [Test]
        public async Task Invoke_WithoutMatchingRedirect_DoesNotRegisterHit()
        {
            // Act
            await _middleware.Invoke(_httpContext);

            // Assert
            _mockRedirectHitTracker.Verify(it => it.RegisterHit(It.IsAny<Guid>()), Times.Never);
        }

        private Redirect SetupRedirect(HttpStatusCode redirectCode)
        {
            var redirect = new Redirect { Key = Guid.NewGuid(), OldUrl = "/old", NewUrl = "/new", RedirectCode = (int)redirectCode };
            _mockRedirectsService.Setup(it => it.GetByUrl(It.IsAny<Uri>()))
                .Returns((Uri uri) => new RedirectFindResult(uri, redirect));
            return redirect;
        }
    }
}
