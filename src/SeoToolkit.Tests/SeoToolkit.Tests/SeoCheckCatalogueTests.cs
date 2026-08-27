using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    /// <summary>
    /// Behaviour of the shipped checks. Each is exercised against real markup rather than a
    /// hand-built PageFacts, so the parser and the check are covered together - a check that
    /// reads a fact the parser never populates would otherwise pass its tests and find nothing.
    /// </summary>
    [TestFixture]
    public class SeoCheckCatalogueTests
    {
        private static readonly Uri PageUrl = new("https://example.com/page");

        private static IReadOnlyList<SeoCheckIssue> Run(ISeoPageCheck check, string html,
            IReadOnlyDictionary<string, object>? options = null,
            string? xRobotsTag = null,
            Action<CrawledResourceBuilder>? configure = null)
        {
            var facts = new AngleSharpPageFactsParser()
                .Parse(html, PageUrl, url => url.Host == "example.com", xRobotsTag);

            var builder = CrawledResourceBuilder.A().Url(PageUrl.AbsoluteUri).Facts(facts);
            configure?.Invoke(builder);

            return check.Run(builder.Build(), new CrawlIndex(), options);
        }

        private static string Page(string head = "", string body = "")
            => $"<html lang=\"en\"><head><title>A perfectly reasonable page title</title>" +
               $"<meta name=\"viewport\" content=\"width=device-width\">{head}</head>" +
               $"<body><h1>Heading</h1>{body}</body></html>";

        // ---- on page --------------------------------------------------------------------------

        [Test]
        public void TitleMissing_FiresOnlyWhenThereIsNoTitle()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Run(new TitleMissingCheck(), "<html><head></head><body></body></html>"),
                    Has.Count.EqualTo(1));
                Assert.That(Run(new TitleMissingCheck(), Page()), Is.Empty);
            });
        }

        [Test]
        public void TitleLength_LeavesAMissingTitleToTheCheckThatOwnsIt()
        {
            //Reporting it here too would count one problem against the score twice.
            Assert.That(Run(new TitleLengthCheck(), "<html><head></head><body></body></html>"), Is.Empty);
        }

        [Test]
        public void TitleLength_ReportsTooLongAndTooShortWithTheMeasurements()
        {
            var longTitle = Run(new TitleLengthCheck(),
                $"<html><head><title>{new string('x', 90)}</title></head><body></body></html>");

            var shortTitle = Run(new TitleLengthCheck(),
                "<html><head><title>Hi</title></head><body></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(longTitle.Single().Variant, Is.EqualTo("TooLong"));
                Assert.That(longTitle.Single().Data["Length"], Is.EqualTo("90"));
                Assert.That(shortTitle.Single().Variant, Is.EqualTo("TooShort"));
            });
        }

        [Test]
        public void TitleLength_HonoursConfiguredThresholds()
        {
            //The old check hard-coded its limits as constants, so a site could not tune them.
            var html = $"<html><head><title>{new string('x', 70)}</title></head><body></body></html>";

            Assert.Multiple(() =>
            {
                Assert.That(Run(new TitleLengthCheck(), html), Has.Count.EqualTo(1));
                Assert.That(Run(new TitleLengthCheck(), html,
                    new Dictionary<string, object> { [TitleLengthCheck.MaxOption] = 100 }), Is.Empty);
            });
        }

        [Test]
        public void MultipleTitles_CountsThem()
        {
            var issues = Run(new MultipleTitlesCheck(),
                "<html><head><title>One</title><title>Two</title></head><body></body></html>");

            Assert.That(issues.Single().Data["Count"], Is.EqualTo("2"));
        }

        [Test]
        public void H1Missing_FiresWhenThereIsNoHeading()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Run(new H1MissingCheck(), "<html><body><p>text</p></body></html>"),
                    Has.Count.EqualTo(1));
                Assert.That(Run(new H1MissingCheck(), Page()), Is.Empty);
            });
        }

        [Test]
        public void HeadingOrder_ReportsASkippedLevelButNotComingBackUp()
        {
            //h3 then h2 starts a new section and is perfectly normal.
            var skipped = Run(new HeadingOrderCheck(), "<html><body><h1>A</h1><h3>B</h3></body></html>");
            var backUp = Run(new HeadingOrderCheck(), "<html><body><h1>A</h1><h2>B</h2><h3>C</h3><h2>D</h2></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(skipped, Has.Count.EqualTo(1));
                Assert.That(skipped.Single().Data["Level"], Is.EqualTo("3"));
                Assert.That(backUp, Is.Empty);
            });
        }

        [Test]
        public void H1DuplicatesTitle_ComparesIgnoringCase()
        {
            var issues = Run(new H1DuplicatesTitleCheck(),
                "<html><head><title>About us</title></head><body><h1>ABOUT US</h1></body></html>");

            Assert.That(issues, Has.Count.EqualTo(1));
        }

        [Test]
        public void MissingViewport_FiresWhenThereIsNoViewportTag()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Run(new MissingViewportCheck(), "<html><head></head><body></body></html>"),
                    Has.Count.EqualTo(1));
                Assert.That(Run(new MissingViewportCheck(), Page()), Is.Empty);
            });
        }

        // ---- indexability ---------------------------------------------------------------------

        [Test]
        public void Noindex_DistinguishesTheMetaTagFromTheHeader()
        {
            //The fix is in a different place for each: markup versus server configuration.
            var meta = Run(new NoindexCheck(), Page(head: "<meta name=\"robots\" content=\"noindex\">"));
            var header = Run(new NoindexCheck(), Page(), xRobotsTag: "noindex");

            Assert.Multiple(() =>
            {
                Assert.That(meta.Single().Variant, Is.EqualTo("Meta"));
                Assert.That(header.Single().Variant, Is.EqualTo("Header"));
                Assert.That(meta.Single().Severity, Is.EqualTo(SeoSeverity.Critical));
            });
        }

        [Test]
        public void Noindex_TreatsTheBareNoneDirectiveAsNoindex()
        {
            Assert.That(Run(new NoindexCheck(), Page(head: "<meta name=\"robots\" content=\"none\">")),
                Has.Count.EqualTo(1));
        }

        [Test]
        public void CanonicalNonSelfReferencing_AcceptsACanonicalPointingAtThisPage()
        {
            var self = Run(new CanonicalNonSelfReferencingCheck(),
                Page(head: "<link rel=\"canonical\" href=\"https://example.com/page\">"));

            var elsewhere = Run(new CanonicalNonSelfReferencingCheck(),
                Page(head: "<link rel=\"canonical\" href=\"https://example.com/other\">"));

            Assert.Multiple(() =>
            {
                Assert.That(self, Is.Empty);
                Assert.That(elsewhere.Single().Data["Canonical"], Is.EqualTo("https://example.com/other"));
            });
        }

        [Test]
        public void MultipleCanonicals_FiresOnConflictingTags()
        {
            var issues = Run(new MultipleCanonicalsCheck(), Page(head:
                "<link rel=\"canonical\" href=\"/a\"><link rel=\"canonical\" href=\"/b\">"));

            Assert.That(issues.Single().Data["Count"], Is.EqualTo("2"));
        }

        // ---- technical ------------------------------------------------------------------------

        [Test]
        public void ClientError_ReportsTheStatusCode()
        {
            var issues = new ClientErrorCheck().Run(
                CrawledResourceBuilder.A().Status(404).Build(), new CrawlIndex());

            Assert.That(issues.Single().Data["StatusCode"], Is.EqualTo("404"));
        }

        [Test]
        public void RedirectChain_AllowsOneHopByDefault()
        {
            var one = new RedirectChainCheck().Run(
                CrawledResourceBuilder.A().Redirects(Hop("https://example.com/a", "https://example.com/b")).Build(),
                new CrawlIndex());

            var two = new RedirectChainCheck().Run(
                CrawledResourceBuilder.A().Redirects(
                    Hop("https://example.com/a", "https://example.com/b"),
                    Hop("https://example.com/b", "https://example.com/c")).Build(),
                new CrawlIndex());

            Assert.Multiple(() =>
            {
                Assert.That(one, Is.Empty);
                Assert.That(two.Single().Data["Hops"], Is.EqualTo("2"));
            });
        }

        [Test]
        public void TemporaryRedirect_FiresOnA302ButNotA301()
        {
            var permanent = new TemporaryRedirectCheck().Run(
                CrawledResourceBuilder.A().Redirects(Hop("https://example.com/a", "https://example.com/b", 301)).Build(),
                new CrawlIndex());

            var temporary = new TemporaryRedirectCheck().Run(
                CrawledResourceBuilder.A().Redirects(Hop("https://example.com/a", "https://example.com/b", 302)).Build(),
                new CrawlIndex());

            Assert.Multiple(() =>
            {
                Assert.That(permanent, Is.Empty);
                Assert.That(temporary.Single().Data["StatusCode"], Is.EqualTo("302"));
            });
        }

        [Test]
        public void MixedContent_OnlyAppliesToASecurePage()
        {
            var html = Page(body: "<img src=\"http://example.com/a.png\">");

            var secure = Run(new MixedContentCheck(), html,
                configure: it => it.Url("https://example.com/page"));

            var insecure = Run(new MixedContentCheck(), html,
                configure: it => it.Url("http://example.com/page"));

            Assert.Multiple(() =>
            {
                Assert.That(secure, Has.Count.EqualTo(1));
                Assert.That(insecure, Is.Empty, "an insecure page is NonHttpsCheck's finding");
            });
        }

        [Test]
        public void SlowResponse_EscalatesToAnErrorWhenVerySlow()
        {
            var slow = new SlowResponseCheck().Run(Timed(3000), new CrawlIndex());
            var verySlow = new SlowResponseCheck().Run(Timed(5000), new CrawlIndex());

            Assert.Multiple(() =>
            {
                Assert.That(slow.Single().Severity, Is.EqualTo(SeoSeverity.Warning));
                Assert.That(verySlow.Single().Severity, Is.EqualTo(SeoSeverity.Error));
                Assert.That(new SlowResponseCheck().Run(Timed(100), new CrawlIndex()), Is.Empty);
            });
        }

        // ---- content and links ------------------------------------------------------------------

        [Test]
        public void ThinContent_CountsVisibleWordsOnly()
        {
            var issues = Run(new ThinContentCheck(),
                "<html><body><p>only a few words</p><script>var a = 'plenty of words in here to pad it out';</script></body></html>");

            Assert.That(issues, Has.Count.EqualTo(1), "script text must not count as content");
        }

        [Test]
        public void PlaceholderContent_FindsDraftTextLeftBehind()
        {
            var issues = Run(new PlaceholderContentCheck(),
                "<html><body><p>Lorem ipsum dolor sit amet, and so on.</p></body></html>");

            Assert.That(issues.Single().Data["Phrase"], Is.EqualTo("lorem ipsum"));
        }

        [Test]
        public void EmptyAnchorText_FindsALinkWithNothingInIt()
        {
            var issues = Run(new EmptyAnchorTextCheck(),
                "<html><body><a href=\"/a\"></a><a href=\"/b\">Fine</a></body></html>");

            Assert.That(issues.Single().Data["Url"], Is.EqualTo("https://example.com/a"));
        }

        [Test]
        public void GenericAnchorText_IgnoresNavigationAndFooter()
        {
            //Chrome repeats the same wording on every page by design; judging it would report the
            //same few links across the whole site.
            var issues = Run(new GenericAnchorTextCheck(),
                "<html><body><nav><a href=\"/a\">Read more</a></nav>" +
                "<main><a href=\"/b\">Click here</a></main></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Data["Text"], Is.EqualTo("Click here"));
            });
        }

        // ---- images and markup -------------------------------------------------------------------

        [Test]
        public void ImageAlt_TreatsAnEmptyAltAsDeliberate()
        {
            //An empty alt marks a decorative image; a missing one is the defect.
            var issues = Run(new ImageAltCheck(),
                "<html><body><img src=\"/a.png\"><img src=\"/b.png\" alt=\"\"></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(issues, Has.Count.EqualTo(1));
                Assert.That(issues.Single().Data["Url"], Is.EqualTo("https://example.com/a.png"));
            });
        }

        [Test]
        public void InvalidJsonLd_ReportsOnlyTheBlockThatDoesNotParse()
        {
            var issues = Run(new InvalidJsonLdCheck(), Page(head:
                "<script type=\"application/ld+json\">{\"@type\":\"Article\"}</script>" +
                "<script type=\"application/ld+json\">{broken}</script>"));

            Assert.That(issues, Has.Count.EqualTo(1));
        }

        [Test]
        public void TwitterCard_StaysQuietWhenOpenGraphIsPresent()
        {
            //Most platforms fall back to Open Graph, so such a page is not without a preview.
            var withOg = Run(new TwitterCardCheck(), Page(head: "<meta property=\"og:title\" content=\"x\">"));
            var withNeither = Run(new TwitterCardCheck(), Page());

            Assert.Multiple(() =>
            {
                Assert.That(withOg, Is.Empty);
                Assert.That(withNeither, Has.Count.EqualTo(1));
            });
        }

        [TestCase("en", true)]
        [TestCase("en-GB", true)]
        [TestCase("x-default", true)]
        [TestCase("english", false)]
        [TestCase("en_GB", false)]
        [TestCase("e", false)]
        public void HreflangCode_ValidatesTheLanguageValue(string code, bool valid)
        {
            var issues = Run(new HreflangCodeCheck(), Page(head:
                $"<link rel=\"alternate\" hreflang=\"{code}\" href=\"/other\">"));

            Assert.That(issues, valid ? Is.Empty : Has.Count.EqualTo(1));
        }

        [Test]
        public void HreflangXDefault_OnlyAppliesWhenThereAreAnnotations()
        {
            var without = Run(new HreflangXDefaultCheck(), Page());
            var missing = Run(new HreflangXDefaultCheck(), Page(head:
                "<link rel=\"alternate\" hreflang=\"nl\" href=\"/nl\">"));

            Assert.Multiple(() =>
            {
                Assert.That(without, Is.Empty);
                Assert.That(missing, Has.Count.EqualTo(1));
            });
        }

        // ---- helpers ------------------------------------------------------------------------------

        private static RedirectHop Hop(string from, string to, int status = 301)
            => new(new Uri(from), new Uri(to), status);

        private static CrawledResource Timed(int ms) => new()
        {
            RequestedUrl = PageUrl,
            FinalUrl = PageUrl,
            NormalizedUrl = PageUrl.AbsoluteUri,
            UrlHash = "hash",
            StatusCode = 200,
            Kind = ResourceKind.HtmlPage,
            IsInternal = true,
            TotalMs = ms
        };
    }
}
