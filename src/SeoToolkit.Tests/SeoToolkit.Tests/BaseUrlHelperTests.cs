using SeoToolkit.Umbraco.Common.Core.Helpers;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class BaseUrlHelperTests
    {
        [Test]
        public void ApplyBaseUrl_NullBaseUrl_ReturnsOriginal()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page", null);
            Assert.That(result, Is.EqualTo("https://api.example.com/page"));
        }

        [Test]
        public void ApplyBaseUrl_EmptyBaseUrl_ReturnsOriginal()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page", "");
            Assert.That(result, Is.EqualTo("https://api.example.com/page"));
        }

        [Test]
        public void ApplyBaseUrl_WithScheme_ReplacesHostAndScheme()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page", "https://www.frontend.com");
            Assert.That(result, Is.EqualTo("https://www.frontend.com/page"));
        }

        [Test]
        public void ApplyBaseUrl_WithoutScheme_UsesHttps()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page", "www.frontend.com");
            Assert.That(result, Is.EqualTo("https://www.frontend.com/page"));
        }

        [Test]
        public void ApplyBaseUrl_PreservesPath()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/en/about-us", "https://www.frontend.com");
            Assert.That(result, Is.EqualTo("https://www.frontend.com/en/about-us"));
        }

        [Test]
        public void ApplyBaseUrl_PreservesQueryString()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page?id=1", "https://www.frontend.com");
            Assert.That(result, Is.EqualTo("https://www.frontend.com/page?id=1"));
        }

        [Test]
        public void ApplyBaseUrl_WithTrailingSlash_NormalizesBaseUrl()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page", "https://www.frontend.com/");
            Assert.That(result, Is.EqualTo("https://www.frontend.com/page"));
        }

        [Test]
        public void ApplyBaseUrl_RelativeUrl_ReturnsOriginal()
        {
            // Relative URLs (not absolute URIs) are returned unchanged
            var result = BaseUrlHelper.ApplyBaseUrl("page-without-scheme", "https://www.frontend.com");
            Assert.That(result, Is.EqualTo("page-without-scheme"));
        }

        [Test]
        public void ApplyBaseUrl_InvalidBaseUrl_ReturnsOriginal()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page", "not a valid url !@#$");
            Assert.That(result, Is.EqualTo("https://api.example.com/page"));
        }

        [Test]
        public void ApplyBaseUrl_DifferentScheme_UsesBaseUrlScheme()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("http://api.example.com/page", "https://www.frontend.com");
            Assert.That(result, Is.EqualTo("https://www.frontend.com/page"));
        }

        [Test]
        public void ApplyBaseUrl_WithPort_PreservesPort()
        {
            var result = BaseUrlHelper.ApplyBaseUrl("https://api.example.com/page", "https://www.frontend.com:8080");
            Assert.That(result, Is.EqualTo("https://www.frontend.com:8080/page"));
        }
    }
}
