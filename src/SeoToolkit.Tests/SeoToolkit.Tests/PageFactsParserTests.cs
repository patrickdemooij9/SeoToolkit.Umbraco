using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class PageFactsParserTests
    {
        private static readonly Uri PageUrl = new("https://example.com/section/page");
        private readonly AngleSharpPageFactsParser _parser = new();

        private PageFacts Parse(string html, string? xRobotsTag = null)
            => _parser.Parse(html, PageUrl, url => url.Host == "example.com", xRobotsTag);

        [Test]
        public void Parse_ReadsTheBasicHeadElements()
        {
            var facts = Parse("""
                <html lang="en-GB"><head>
                    <title>  My   page  </title>
                    <meta name="description" content="A description">
                    <meta name="viewport" content="width=device-width">
                    <link rel="canonical" href="/section/page">
                </head><body><h1>Heading</h1></body></html>
                """);

            Assert.Multiple(() =>
            {
                Assert.That(facts.Title, Is.EqualTo("My page"), "whitespace is collapsed");
                Assert.That(facts.MetaDescription, Is.EqualTo("A description"));
                Assert.That(facts.Lang, Is.EqualTo("en-GB"));
                Assert.That(facts.Viewport, Is.EqualTo("width=device-width"));
                Assert.That(facts.PrimaryCanonical, Is.EqualTo(new Uri("https://example.com/section/page")));
                Assert.That(facts.PrimaryH1, Is.EqualTo("Heading"));
            });
        }

        [Test]
        public void Parse_CountsDuplicatedHeadElements()
        {
            //Two titles is a defect, and only a count can reveal it.
            var facts = Parse("<html><head><title>One</title><title>Two</title>" +
                              "<meta name='description' content='a'><meta name='description' content='b'>" +
                              "</head><body></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(facts.TitleCount, Is.EqualTo(2));
                Assert.That(facts.MetaDescriptionCount, Is.EqualTo(2));
            });
        }

        [Test]
        public void Parse_ReturnsNullForAnEmptyTitleRatherThanAnEmptyString()
        {
            var facts = Parse("<html><head><title>   </title></head><body></body></html>");

            Assert.That(facts.Title, Is.Null);
        }

        [Test]
        public void Parse_KeepsHeadingsInDocumentOrder()
        {
            //Detecting a skipped level is only possible with order preserved.
            var facts = Parse("<html><body><h1>A</h1><h3>B</h3><h2>C</h2></body></html>");

            Assert.That(facts.Headings.Select(it => it.Level), Is.EqualTo(new[] { 1, 3, 2 }));
        }

        [Test]
        public void Parse_ResolvesRelativeLinksAgainstThePageUrl()
        {
            var facts = Parse("""
                <html><body>
                    <a href="../other">Up</a>
                    <a href="/absolute">Root</a>
                    <a href="https://external.com/x">External</a>
                </body></html>
                """);

            Assert.That(facts.Links.Select(it => it.Url.AbsoluteUri), Is.EqualTo(new[]
            {
                "https://example.com/other",
                "https://example.com/absolute",
                "https://external.com/x"
            }));
        }

        [Test]
        public void Parse_MarksLinksAsInternalOrExternal()
        {
            var facts = Parse("<html><body><a href='/a'>A</a><a href='https://external.com/b'>B</a></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(facts.Links[0].IsInternal, Is.True);
                Assert.That(facts.Links[1].IsInternal, Is.False);
            });
        }

        [Test]
        public void Parse_ReadsAnchorTextAndRelTokens()
        {
            var facts = Parse("<html><body><a href='/a' rel='nofollow noopener'>  Click   here </a></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(facts.Links[0].AnchorText, Is.EqualTo("Click here"));
                Assert.That(facts.Links[0].IsNoFollow, Is.True);
                Assert.That(facts.Links[0].IsSponsored, Is.False);
            });
        }

        [Test]
        public void Parse_DoesNotMatchARelTokenThatIsMerelyASubstring()
        {
            var facts = Parse("<html><body><a href='/a' rel='nofollowing'>x</a></body></html>");

            Assert.That(facts.Links[0].IsNoFollow, Is.False);
        }

        [Test]
        public void Parse_AssignsLinksToTheirNearestLandmark()
        {
            var facts = Parse("""
                <html><body>
                    <nav><a href="/nav">Nav</a></nav>
                    <main><a href="/body">Body</a></main>
                    <footer><a href="/foot">Foot</a></footer>
                </body></html>
                """);

            Assert.That(facts.Links.Select(it => it.Region),
                Is.EqualTo(new[] { LinkRegion.Navigation, LinkRegion.Body, LinkRegion.Footer }));
        }

        [Test]
        public void Parse_DistinguishesAMissingAltFromAnEmptyOne()
        {
            //An empty alt marks a decorative image; a missing one is an accessibility defect.
            var facts = Parse("""
                <html><body>
                    <img src="/a.png">
                    <img src="/b.png" alt="">
                    <img src="/c.png" alt="A cat" width="800" height="600" loading="lazy">
                </body></html>
                """);

            Assert.Multiple(() =>
            {
                Assert.That(facts.Images[0].HasAltAttribute, Is.False);
                Assert.That(facts.Images[1].HasAltAttribute, Is.True);
                Assert.That(facts.Images[1].Alt, Is.Empty);
                Assert.That(facts.Images[2].Alt, Is.EqualTo("A cat"));
                Assert.That(facts.Images[2].HasDimensions, Is.True);
                Assert.That(facts.Images[2].IsLazyLoaded, Is.True);
            });
        }

        [Test]
        public void Parse_CombinesRobotsMetaAndHeaderDirectives()
        {
            var facts = Parse("<html><head><meta name='robots' content='noindex, follow'></head><body></body></html>",
                xRobotsTag: "noarchive");

            Assert.Multiple(() =>
            {
                Assert.That(facts.MetaRobots.HasFlag(RobotsDirectives.NoIndex), Is.True);
                Assert.That(facts.XRobotsTag.HasFlag(RobotsDirectives.NoArchive), Is.True);
                Assert.That(facts.EffectiveRobots.BlocksIndexing(), Is.True);
                Assert.That(facts.EffectiveRobots.BlocksFollowing(), Is.False);
            });
        }

        [Test]
        public void Parse_ExpandsTheBareNoneDirective()
        {
            //"none" is shorthand for noindex, nofollow.
            var facts = Parse("<html><head><meta name='robots' content='none'></head><body></body></html>");

            Assert.Multiple(() =>
            {
                Assert.That(facts.EffectiveRobots.BlocksIndexing(), Is.True);
                Assert.That(facts.EffectiveRobots.BlocksFollowing(), Is.True);
            });
        }

        [Test]
        public void Parse_ReadsHreflangIncludingXDefault()
        {
            var facts = Parse("""
                <html><head>
                    <link rel="alternate" hreflang="nl" href="/nl/page">
                    <link rel="alternate" hreflang="x-default" href="/page">
                </head><body></body></html>
                """);

            Assert.Multiple(() =>
            {
                Assert.That(facts.HreflangEntries, Has.Count.EqualTo(2));
                Assert.That(facts.HreflangEntries[1].IsXDefault, Is.True);
            });
        }

        [Test]
        public void Parse_ReadsOpenGraphAndTwitterCards()
        {
            var facts = Parse("""
                <html><head>
                    <meta property="og:title" content="OG title">
                    <meta property="og:image" content="/og.png">
                    <meta name="twitter:card" content="summary_large_image">
                </head><body></body></html>
                """);

            Assert.Multiple(() =>
            {
                Assert.That(facts.OpenGraph.Title, Is.EqualTo("OG title"));
                Assert.That(facts.OpenGraph.Image, Is.EqualTo(new Uri("https://example.com/og.png")));
                Assert.That(facts.TwitterCard.Card, Is.EqualTo("summary_large_image"));
                Assert.That(facts.TwitterCard.IsEmpty, Is.False);
            });
        }

        [Test]
        public void Parse_FlagsInvalidJsonLdWithoutThrowing()
        {
            var facts = Parse("""
                <html><head>
                    <script type="application/ld+json">{"@type":"Article","headline":"x"}</script>
                    <script type="application/ld+json">{not json}</script>
                </head><body></body></html>
                """);

            Assert.Multiple(() =>
            {
                Assert.That(facts.JsonLdBlocks[0].IsValid, Is.True);
                Assert.That(facts.JsonLdBlocks[0].Type, Is.EqualTo("Article"));
                Assert.That(facts.JsonLdBlocks[1].IsValid, Is.False);
            });
        }

        [Test]
        public void Parse_ExcludesScriptAndStyleFromVisibleText()
        {
            //Counting script bodies would make a script-heavy page look content-rich.
            var facts = Parse("""
                <html><body>
                    <p>Real content here</p>
                    <script>var a = "lots and lots of words in a script";</script>
                    <style>.x { color: red; }</style>
                </body></html>
                """);

            Assert.Multiple(() =>
            {
                Assert.That(facts.TextContent, Is.EqualTo("Real content here"));
                Assert.That(facts.WordCount, Is.EqualTo(3));
            });
        }

        [Test]
        public void Parse_CollectsScriptsAndStylesheets()
        {
            var facts = Parse("""
                <html><head>
                    <link rel="stylesheet" href="/site.css">
                    <script src="/app.js"></script>
                </head><body></body></html>
                """);

            Assert.Multiple(() =>
            {
                Assert.That(facts.Stylesheets.Single().AbsoluteUri, Is.EqualTo("https://example.com/site.css"));
                Assert.That(facts.Scripts.Single().AbsoluteUri, Is.EqualTo("https://example.com/app.js"));
            });
        }

        [Test]
        public void ContentHash_MatchesForIdenticalTextAndDiffersOtherwise()
        {
            var a = AngleSharpPageFactsParser.ComputeContentHash("the same words");
            var b = AngleSharpPageFactsParser.ComputeContentHash("the same words");
            var c = AngleSharpPageFactsParser.ComputeContentHash("different words");

            Assert.Multiple(() =>
            {
                Assert.That(b, Is.EqualTo(a));
                Assert.That(c, Is.Not.EqualTo(a));
            });
        }

        [Test]
        public void ContentHash_IsZeroOnlyForEmptyContent()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AngleSharpPageFactsParser.ComputeContentHash(null), Is.Zero);
                Assert.That(AngleSharpPageFactsParser.ComputeContentHash(""), Is.Zero);
                Assert.That(AngleSharpPageFactsParser.ComputeContentHash("x"), Is.Not.Zero);
            });
        }

        [Test]
        public void Parse_SurvivesMalformedMarkup()
        {
            //Real sites ship broken html; the parser has to recover the way a browser would.
            var facts = Parse("<html><body><p>Unclosed <b>bold<div><h1>Heading</body>");

            Assert.That(facts.PrimaryH1, Is.EqualTo("Heading"));
        }

        [Test]
        public void Parse_HandlesAnEmptyDocument()
        {
            var facts = Parse(string.Empty);

            Assert.Multiple(() =>
            {
                Assert.That(facts.Title, Is.Null);
                Assert.That(facts.Links, Is.Empty);
                Assert.That(facts.Images, Is.Empty);
                Assert.That(facts.WordCount, Is.Zero);
            });
        }

        [Test]
        public void Parse_IgnoresLinksThatAreNotResolvable()
        {
            var facts = Parse("<html><body><a href=''>Empty</a><a href='/ok'>Ok</a></body></html>");

            Assert.That(facts.Links.Select(it => it.Url.AbsoluteUri),
                Is.EqualTo(new[] { "https://example.com/ok" }));
        }
    }
}
