using HtmlAgilityPack;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SiteAuditChecksTests
    {
        [Test]
        public void MissingH1Check_WhenH1Missing_ReturnsError()
        {
            var check = new MissingH1Check();
            var page = CreatePage("<html><head></head><body><p>Test</p></body></html>");

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Result, Is.EqualTo(SiteCrawlResultType.Error));
        }

        [Test]
        public void MissingH1Check_WhenH1Exists_ReturnsNoResults()
        {
            var check = new MissingH1Check();
            var page = CreatePage("<html><head></head><body><h1>Main heading</h1></body></html>");

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ThinContentCheck_WhenContentIsThin_ReturnsWarning()
        {
            var check = new ThinContentCheck();
            var page = CreatePage("<html><head></head><body><p>few words only</p></body></html>");

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Result, Is.EqualTo(SiteCrawlResultType.Warning));
        }

        [Test]
        public void ThinContentCheck_WhenContentIsSubstantial_ReturnsNoResults()
        {
            var check = new ThinContentCheck();
            var repeatedText = string.Join(" ", Enumerable.Repeat("word", 180));
            var page = CreatePage($"<html><head></head><body><p>{repeatedText}</p></body></html>");

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void MissingCanonicalCheck_WhenCanonicalMissing_ReturnsWarning()
        {
            var check = new MissingCanonicalCheck();
            var page = CreatePage("<html><head><title>Test</title></head><body></body></html>");

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Result, Is.EqualTo(SiteCrawlResultType.Warning));
        }

        [Test]
        public void MissingCanonicalCheck_WhenCanonicalExists_ReturnsNoResults()
        {
            var check = new MissingCanonicalCheck();
            var page = CreatePage("<html><head><link rel=\"canonical\" href=\"https://example.com/test\" /></head><body></body></html>");

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CoreWebVitalsCheck_WhenResponseIsSlow_ReturnsWarning()
        {
            var check = new PagePerformanceCheck();
            var page = CreatePage("<html><head></head><body></body></html>");
            page.RequestStarted = DateTime.UtcNow;
            page.RequestCompleted = page.RequestStarted.AddMilliseconds(3000);

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Result, Is.EqualTo(SiteCrawlResultType.Warning));
        }

        [Test]
        public void CoreWebVitalsCheck_WhenResponseIsPoor_ReturnsError()
        {
            var check = new PagePerformanceCheck();
            var page = CreatePage("<html><head></head><body></body></html>");
            page.RequestStarted = DateTime.UtcNow;
            page.RequestCompleted = page.RequestStarted.AddMilliseconds(4500);

            var result = check.RunCheck(page, new SiteAuditContext()).ToArray();

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Result, Is.EqualTo(SiteCrawlResultType.Error));
        }

        private static CrawledPageModel CreatePage(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return new CrawledPageModel(new Uri("https://example.com/page"))
            {
                Content = doc,
                StatusCode = 200
            };
        }
    }
}
