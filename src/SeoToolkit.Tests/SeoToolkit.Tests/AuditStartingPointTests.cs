using System.Net.Http;
using System.Reflection;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Legacy;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Tests
{
    /// <summary>
    /// Turning what someone typed into a url the crawl can start from.
    /// <para>
    /// These are the rules for the second way of starting an audit. A decoupled frontend can
    /// route however it likes, so there may be no node whose url resembles the page that is
    /// actually served - which makes typing a url the only workable option for such a site.
    /// </para>
    /// </summary>
    [TestFixture]
    public class CrawlUrlParsingTests
    {
        private static Uri? Parse(string? value)
        {
            AuditStartingPointResolver.TryParseCrawlUrl(value, out var url, out _);
            return url;
        }

        private static string? Error(string? value)
        {
            AuditStartingPointResolver.TryParseCrawlUrl(value, out _, out var error);
            return error;
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void NothingTyped_IsRejectedWithSomethingToAct_On(string? value)
        {
            Assert.Multiple(() =>
            {
                Assert.That(Parse(value), Is.Null);
                Assert.That(Error(value), Is.Not.Null.And.Not.Empty);
            });
        }

        [Test]
        public void BareHost_GetsHttps()
        {
            //Typing example.com is a reasonable thing to do, and refusing it would be pedantic.
            Assert.That(Parse("example.com")?.AbsoluteUri, Is.EqualTo("https://example.com/"));
        }

        [Test]
        public void ExplicitHttp_IsLeftAlone()
        {
            //Some decoupled frontends genuinely are on plain http in staging.
            Assert.That(Parse("http://example.com/")?.AbsoluteUri, Is.EqualTo("http://example.com/"));
        }

        [Test]
        public void PathPortAndQuery_AreKept()
        {
            Assert.That(Parse("https://front.example.com:8443/en/shop?page=2")?.AbsoluteUri,
                Is.EqualTo("https://front.example.com:8443/en/shop?page=2"));
        }

        [Test]
        public void Fragment_IsDropped()
        {
            //A fragment never identifies a different page to the server, so carrying one in
            //would only make the starting url unlike every other url in the run.
            Assert.That(Parse("https://example.com/page#section")?.AbsoluteUri,
                Is.EqualTo("https://example.com/page"));
        }

        [Test]
        public void SurroundingWhitespace_IsForgiven()
        {
            Assert.That(Parse("  https://example.com/  ")?.AbsoluteUri, Is.EqualTo("https://example.com/"));
        }

        [TestCase("ftp://example.com/")]
        [TestCase("file:///c:/temp")]
        [TestCase("javascript:alert(1)")]
        public void SchemesTheCrawlerCannotFetch_AreRejected(string value)
        {
            Assert.Multiple(() =>
            {
                Assert.That(Parse(value), Is.Null);
                Assert.That(Error(value), Is.Not.Null.And.Not.Empty);
            });
        }

        [Test]
        public void SomethingThatIsNotAUrl_IsRejected()
        {
            Assert.That(Parse("not a url !@#$ %"), Is.Null);
        }
    }
}
