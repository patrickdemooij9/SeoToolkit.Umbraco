using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class UrlNormalizerTests
    {
        private static DefaultUrlNormalizer Normalizer(CrawlOptions? options = null)
            => new(options ?? new CrawlOptions());

        [Test]
        public void Normalize_DropsTheFragment()
        {
            //The fragment is never sent to the server, so these are one page - the old crawler
            //treated them as two and re-ran every check on the same content.
            var normalizer = Normalizer();

            Assert.That(normalizer.Normalize(new Uri("https://example.com/page#section")),
                Is.EqualTo(normalizer.Normalize(new Uri("https://example.com/page"))));
        }

        [Test]
        public void Normalize_LowercasesSchemeAndHostButNotPath()
        {
            //Hosts are case insensitive; paths are not.
            var result = Normalizer().Normalize(new Uri("HTTPS://Example.COM/MyPage"));

            Assert.That(result, Is.EqualTo("https://example.com/MyPage"));
        }

        [Test]
        public void Normalize_RemovesTheDefaultPort()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Normalizer().Normalize(new Uri("https://example.com:443/a")),
                    Is.EqualTo("https://example.com/a"));
                Assert.That(Normalizer().Normalize(new Uri("http://example.com:8080/a")),
                    Is.EqualTo("http://example.com:8080/a"));
            });
        }

        [Test]
        public void Normalize_ResolvesDotSegments()
        {
            Assert.That(Normalizer().Normalize(new Uri("https://example.com/a/b/../c")),
                Is.EqualTo("https://example.com/a/c"));
        }

        [Test]
        public void Normalize_ConvertsUnicodeHostsToPunycode()
        {
            Assert.That(Normalizer().Normalize(new Uri("https://bücher.example/a")),
                Is.EqualTo("https://xn--bcher-kva.example/a"));
        }

        [Test]
        public void Normalize_StripsTrackingParametersByDefault()
        {
            var result = Normalizer().Normalize(
                new Uri("https://example.com/p?utm_source=news&utm_campaign=x&gclid=123&id=7"));

            Assert.That(result, Is.EqualTo("https://example.com/p?id=7"));
        }

        [Test]
        public void Normalize_DropsTheQuestionMarkWhenEveryParameterWasStripped()
        {
            Assert.That(Normalizer().Normalize(new Uri("https://example.com/p?utm_source=news")),
                Is.EqualTo("https://example.com/p"));
        }

        [Test]
        public void Normalize_SortsRemainingParametersSoOrderDoesNotMatter()
        {
            var normalizer = Normalizer();

            Assert.That(normalizer.Normalize(new Uri("https://example.com/p?b=2&a=1")),
                Is.EqualTo(normalizer.Normalize(new Uri("https://example.com/p?a=1&b=2"))));
        }

        [Test]
        public void Normalize_CanKeepParameterOrderWhenAskedTo()
        {
            var normalizer = Normalizer(new CrawlOptions { SortQueryParameters = false });

            Assert.That(normalizer.Normalize(new Uri("https://example.com/p?b=2&a=1")),
                Is.EqualTo("https://example.com/p?b=2&a=1"));
        }

        [Test]
        public void Normalize_RemovesTrailingSlashesByDefault()
        {
            var normalizer = Normalizer();

            Assert.That(normalizer.Normalize(new Uri("https://example.com/about/")),
                Is.EqualTo(normalizer.Normalize(new Uri("https://example.com/about"))));
        }

        [Test]
        public void Normalize_NeverStripsTheRootSlash()
        {
            Assert.That(Normalizer().Normalize(new Uri("https://example.com/")),
                Is.EqualTo("https://example.com/"));
        }

        [Test]
        public void Normalize_CanPreserveTrailingSlashes()
        {
            var normalizer = Normalizer(new CrawlOptions { TrailingSlash = TrailingSlashPolicy.Preserve });

            Assert.That(normalizer.Normalize(new Uri("https://example.com/about/")),
                Is.EqualTo("https://example.com/about/"));
        }

        [Test]
        public void Normalize_CanAddTrailingSlashesToDirectoriesButNotFiles()
        {
            var normalizer = Normalizer(new CrawlOptions { TrailingSlash = TrailingSlashPolicy.Add });

            Assert.Multiple(() =>
            {
                Assert.That(normalizer.Normalize(new Uri("https://example.com/about")),
                    Is.EqualTo("https://example.com/about/"));
                Assert.That(normalizer.Normalize(new Uri("https://example.com/file.pdf")),
                    Is.EqualTo("https://example.com/file.pdf"));
            });
        }

        [TestCase("mailto:someone@example.com")]
        [TestCase("tel:+3112345678")]
        [TestCase("javascript:void(0)")]
        [TestCase("ftp://example.com/file")]
        public void Normalize_RejectsAnythingThatIsNotAWebPage(string url)
        {
            Assert.That(Normalizer().Normalize(new Uri(url)), Is.Null);
        }

        [Test]
        public void Hash_IsStableAndDistinct()
        {
            var normalizer = Normalizer();

            Assert.Multiple(() =>
            {
                var first = normalizer.Hash("https://example.com/a");
                var again = normalizer.Hash("https://example.com/a");
                var other = normalizer.Hash("https://example.com/b");

                Assert.That(again, Is.EqualTo(first));
                Assert.That(other, Is.Not.EqualTo(first));
            });
        }
    }

    [TestFixture]
    public class RobotsTxtMatcherTests
    {
        private const string Agent = "SeoToolkit-SiteAudit/7.0";

        [Test]
        public void Parse_WithNoContent_AllowsEverything()
        {
            Assert.That(RobotsTxtMatcher.Parse(null).IsAllowed(Agent, "/anything"), Is.True);
        }

        [Test]
        public void IsAllowed_HonoursASimpleDisallow()
        {
            var matcher = RobotsTxtMatcher.Parse("User-agent: *\nDisallow: /admin");

            Assert.Multiple(() =>
            {
                Assert.That(matcher.IsAllowed(Agent, "/admin/users"), Is.False);
                Assert.That(matcher.IsAllowed(Agent, "/public"), Is.True);
            });
        }

        [Test]
        public void IsAllowed_TreatsAnEmptyDisallowAsAllowingEverything()
        {
            var matcher = RobotsTxtMatcher.Parse("User-agent: *\nDisallow:");

            Assert.That(matcher.IsAllowed(Agent, "/anything"), Is.True);
        }

        [Test]
        public void IsAllowed_LetsTheLongestMatchWin()
        {
            //"Block /admin, except /admin/public" has to mean what it looks like.
            var matcher = RobotsTxtMatcher.Parse(
                "User-agent: *\nDisallow: /admin\nAllow: /admin/public");

            Assert.Multiple(() =>
            {
                Assert.That(matcher.IsAllowed(Agent, "/admin/secret"), Is.False);
                Assert.That(matcher.IsAllowed(Agent, "/admin/public/page"), Is.True);
            });
        }

        [Test]
        public void IsAllowed_PrefersAllowWhenTwoRulesAreEquallySpecific()
        {
            var matcher = RobotsTxtMatcher.Parse("User-agent: *\nDisallow: /x\nAllow: /x");

            Assert.That(matcher.IsAllowed(Agent, "/x"), Is.True);
        }

        [Test]
        public void IsAllowed_SupportsWildcards()
        {
            var matcher = RobotsTxtMatcher.Parse("User-agent: *\nDisallow: /*.pdf");

            Assert.Multiple(() =>
            {
                Assert.That(matcher.IsAllowed(Agent, "/docs/manual.pdf"), Is.False);
                Assert.That(matcher.IsAllowed(Agent, "/docs/manual.html"), Is.True);
            });
        }

        [Test]
        public void IsAllowed_SupportsEndOfPathAnchoring()
        {
            var matcher = RobotsTxtMatcher.Parse("User-agent: *\nDisallow: /*.php$");

            Assert.Multiple(() =>
            {
                Assert.That(matcher.IsAllowed(Agent, "/index.php"), Is.False);
                Assert.That(matcher.IsAllowed(Agent, "/index.php?id=1"), Is.True);
            });
        }

        [Test]
        public void IsAllowed_ObeysExactlyOneGroup_NotTheUnionOfAllOfThem()
        {
            var matcher = RobotsTxtMatcher.Parse(
                "User-agent: *\nDisallow: /everyone\n\nUser-agent: SeoToolkit\nDisallow: /justus");

            Assert.Multiple(() =>
            {
                Assert.That(matcher.IsAllowed(Agent, "/justus"), Is.False, "our own group applies");
                Assert.That(matcher.IsAllowed(Agent, "/everyone"), Is.True,
                    "the wildcard group is superseded, not combined");
            });
        }

        [Test]
        public void IsAllowed_FallsBackToTheWildcardGroupForAnUnknownAgent()
        {
            var matcher = RobotsTxtMatcher.Parse(
                "User-agent: *\nDisallow: /everyone\n\nUser-agent: Googlebot\nDisallow: /google");

            Assert.Multiple(() =>
            {
                Assert.That(matcher.IsAllowed(Agent, "/everyone"), Is.False);
                Assert.That(matcher.IsAllowed(Agent, "/google"), Is.True);
            });
        }

        [Test]
        public void Parse_SharesRulesAcrossConsecutiveUserAgentLines()
        {
            var matcher = RobotsTxtMatcher.Parse(
                "User-agent: Googlebot\nUser-agent: SeoToolkit\nDisallow: /shared");

            Assert.That(matcher.IsAllowed(Agent, "/shared"), Is.False);
        }

        [Test]
        public void Parse_IgnoresComments()
        {
            var matcher = RobotsTxtMatcher.Parse("User-agent: * # everyone\nDisallow: /admin # keep out");

            Assert.Multiple(() =>
            {
                Assert.That(matcher.IsAllowed(Agent, "/admin"), Is.False);
                Assert.That(matcher.IsAllowed(Agent, "/other"), Is.True);
            });
        }

        [Test]
        public void Parse_CollectsSitemapDeclarations()
        {
            var matcher = RobotsTxtMatcher.Parse(
                "Sitemap: https://example.com/sitemap.xml\nUser-agent: *\nDisallow:\nSitemap: https://example.com/news.xml");

            Assert.That(matcher.Sitemaps, Is.EqualTo(new[]
            {
                "https://example.com/sitemap.xml",
                "https://example.com/news.xml"
            }));
        }

        [Test]
        public void GetCrawlDelaySeconds_ReadsTheDelayForTheMatchedGroup()
        {
            var matcher = RobotsTxtMatcher.Parse("User-agent: *\nCrawl-delay: 3\nDisallow:");

            Assert.That(matcher.GetCrawlDelaySeconds(Agent), Is.EqualTo(3));
        }

        [Test]
        public void Parse_ToleratesJunkLines()
        {
            var matcher = RobotsTxtMatcher.Parse("this is not robots.txt\n\n???\nUser-agent: *\nDisallow: /x");

            Assert.That(matcher.IsAllowed(Agent, "/x"), Is.False);
        }
    }
}
