using System.Net;
using System.Net.Http;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Legacy;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class LegacySiteCheckAdapterTests
    {
        private const string Root = "https://example.com/";

        [Test]
        public void Descriptor_IsDerivedFromTheOldCheck()
        {
            var adapter = new LegacySiteCheckAdapter(new LegacyH1Check());

            Assert.Multiple(() =>
            {
                Assert.That(adapter.Descriptor.Alias, Is.EqualTo("LegacyH1Check"));
                Assert.That(adapter.Descriptor.Name, Is.EqualTo("Legacy H1 check"));
                Assert.That(adapter.Descriptor.ProviderName, Is.EqualTo(LegacySiteCheckAdapter.LegacyProviderName));
            });
        }

        [Test]
        public void Descriptor_DeclaresThatItNeedsTheRawBody()
        {
            //Old checks expect an HtmlAgilityPack document, so the markup has to be retained.
            //Declaring it means that cost is only paid when a legacy check is actually registered.
            var adapter = new LegacySiteCheckAdapter(new LegacyH1Check());

            Assert.That(adapter.Descriptor.RequiredCapabilities.Requires(SeoCheckCapabilities.RawBody), Is.True);
        }

        [Test]
        public async Task RunAsync_TranslatesAnOldFailureIntoAnIssue()
        {
            var adapter = new LegacySiteCheckAdapter(new LegacyH1Check());
            var sink = new RecordingIssueSink();

            await adapter.RunAsync(Context(adapter, "<html><head></head><body><p>No heading</p></body></html>", sink),
                CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(sink.Issues, Has.Count.EqualTo(1));
                Assert.That(sink.Issues[0].CheckAlias, Is.EqualTo("LegacyH1Check"));
                Assert.That(sink.Issues[0].Severity, Is.EqualTo(SeoSeverity.Error));
            });
        }

        [Test]
        public async Task RunAsync_ReportsNothingWhenTheOldCheckPasses()
        {
            var adapter = new LegacySiteCheckAdapter(new LegacyH1Check());
            var sink = new RecordingIssueSink();

            await adapter.RunAsync(Context(adapter, "<html><body><h1>Heading</h1></body></html>", sink), CancellationToken.None);

            Assert.That(sink.Issues, Is.Empty);
        }

        [Test]
        public async Task RunAsync_KeepsTheOldFormattedMessageAsEvidence()
        {
            var adapter = new LegacySiteCheckAdapter(new RecordingLegacyCheck());
            var sink = new RecordingIssueSink();

            await adapter.RunAsync(Context(adapter, "<html><body></body></html>", sink), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(sink.Issues[0].Evidence, Is.EqualTo("formatted: value"));
                Assert.That(sink.Issues[0].Data["key"], Is.EqualTo("value"));
            });
        }

        [Test]
        public async Task RunAsync_DoesNothingWhenTheBodyWasNotRetained()
        {
            var adapter = new LegacySiteCheckAdapter(new LegacyH1Check());
            var sink = new RecordingIssueSink();

            var resource = CrawledResourceBuilder.A().Build(); // no RawBody
            var context = new SeoPageCheckContext(resource, adapter.Descriptor, SeoCheckOptions.Empty,
                new FakeRunContext(), sink, new CrawlIndex());

            await adapter.RunAsync(context, CancellationToken.None);

            Assert.That(sink.Issues, Is.Empty);
        }

        [Test]
        public async Task RunAsync_SurvivesAnOldCheckThatCannotFormatItsOwnMessage()
        {
            var adapter = new LegacySiteCheckAdapter(new BadFormatterLegacyCheck());
            var sink = new RecordingIssueSink();

            await adapter.RunAsync(Context(adapter, "<html><body></body></html>", sink), CancellationToken.None);

            //The finding still counts even though the wording could not be produced.
            Assert.That(sink.Issues, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task LegacyContext_AnswersFromTheCrawlIndex()
        {
            //An old check asking "have we already seen this url" must get the real answer,
            //without anything copying the whole crawl into a dictionary.
            var index = new CrawlIndex();
            SeoCheckApiTests.AddPage(index, "https://example.com/known", statusCode: 404);

            var check = new ContextProbingLegacyCheck(new Uri("https://example.com/known"));
            var adapter = new LegacySiteCheckAdapter(check);
            var sink = new RecordingIssueSink();

            await adapter.RunAsync(Context(adapter, "<html><body></body></html>", sink, index), CancellationToken.None);

            Assert.That(check.ObservedStatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task LegacyChecksStillWorkEndToEndOnTheNewEngine()
        {
            var site = new FakeSiteHandler()
                .Page(Root, "<html><head><title>A page with a decent title</title></head>" +
                            "<body><p>No heading here at all</p></body></html>");

            var options = new CrawlOptions { MaxConcurrency = 2, UserAgent = "SeoToolkit-Tests" };
            var client = new HttpClient(site);
            var engine = new CrawlEngine(
                new HttpResourceFetcher(client, options),
                new DefaultUrlNormalizer(options),
                new AngleSharpPageFactsParser(),
                new HostPolitenessGate(TimeSpan.Zero));

            var checks = new ISeoCheck[] { new LegacySiteCheckAdapter(new LegacyH1Check()) };

            var result = await engine.CrawlAsync(new Uri(Root), options, checks,
                new FakeRunContext
                {
                    AvailableCapabilities = SeoCheckCapabilities.CrawlIndex | SeoCheckCapabilities.RawBody
                });

            Assert.Multiple(() =>
            {
                Assert.That(result.Crawled, Is.EqualTo(1));
                Assert.That(result.Issues, Has.Count.EqualTo(1));
                Assert.That(result.Issues[0].CheckAlias, Is.EqualTo("LegacyH1Check"));
            });
        }

        /// <summary>
        /// Builds the context the way the engine does, which includes handing the check its own
        /// descriptor - that is what makes reported issues carry the right alias.
        /// </summary>
        private static SeoPageCheckContext Context(LegacySiteCheckAdapter adapter, string html,
            RecordingIssueSink sink, CrawlIndex? index = null)
        {
            var url = new Uri("https://example.com/page");
            var resource = new CrawledResource
            {
                RequestedUrl = url,
                FinalUrl = url,
                NormalizedUrl = url.AbsoluteUri,
                UrlHash = "hash",
                StatusCode = 200,
                Kind = ResourceKind.HtmlPage,
                IsInternal = true,
                RawBody = html,
                Facts = new AngleSharpPageFactsParser().Parse(html, url, _ => true)
            };

            return new SeoPageCheckContext(resource, adapter.Descriptor, SeoCheckOptions.Empty,
                new FakeRunContext(), sink, index ?? new CrawlIndex());
        }

        // ---- legacy checks used only by these tests -------------------------------------------

        /// <summary>
        /// A check in the shape the old api expected. Written here rather than reusing a shipped
        /// one, because the point is that the adapter runs *any* ISiteCheck - the package's own
        /// checks have all been ported to the new api.
        /// </summary>
        private sealed class LegacyH1Check : ISiteCheck
        {
            public string Name => "Legacy H1 check";
            public string Alias => "LegacyH1Check";
            public string Description => "Checks that a page has an H1";
            public string ErrorMessage => "Your site has pages without an H1!";

            public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
            {
                var headings = page.Content?.DocumentNode?.SelectNodes("//h1");
                if (headings is null || headings.Count == 0)
                    yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Error };
            }

            public string FormatMessage(CheckPageCrawlResult crawlResult) => ErrorMessage;

            public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult) => false;
        }

        private sealed class RecordingLegacyCheck : ISiteCheck
        {
            public string Name => "Recording";
            public string Alias => "RecordingLegacyCheck";
            public string Description => "Always fails";
            public string ErrorMessage => "It failed";

            public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
            {
                yield return new CheckPageCrawlResult
                {
                    Result = SiteCrawlResultType.Warning,
                    ExtraValues = new Dictionary<string, string> { ["key"] = "value" }
                };
            }

            public string FormatMessage(CheckPageCrawlResult crawlResult)
                => $"formatted: {crawlResult.ExtraValues["key"]}";

            public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult) => false;
        }

        private sealed class BadFormatterLegacyCheck : ISiteCheck
        {
            public string Name => "Bad formatter";
            public string Alias => "BadFormatterLegacyCheck";
            public string Description => "Throws while formatting";
            public string ErrorMessage => "It failed";

            public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
            {
                yield return new CheckPageCrawlResult { Result = SiteCrawlResultType.Error };
            }

            public string FormatMessage(CheckPageCrawlResult crawlResult)
                => throw new KeyNotFoundException("missing extra value");

            public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult) => false;
        }

        private sealed class ContextProbingLegacyCheck : ISiteCheck
        {
            private readonly Uri _probe;

            public ContextProbingLegacyCheck(Uri probe) => _probe = probe;

            public int? ObservedStatusCode { get; private set; }

            public string Name => "Context probe";
            public string Alias => "ContextProbingLegacyCheck";
            public string Description => "Reads the shared context";
            public string ErrorMessage => "n/a";

            public IEnumerable<CheckPageCrawlResult> RunCheck(CrawledPageModel page, SiteAuditContext context)
            {
                ObservedStatusCode = context.GetStatusCode(_probe);
                return Array.Empty<CheckPageCrawlResult>();
            }

            public string FormatMessage(CheckPageCrawlResult crawlResult) => string.Empty;

            public bool Compare(CheckPageCrawlResult result, CheckPageCrawlResult otherResult) => false;
        }
    }

    [TestFixture]
    public class SiteEntryPointProviderTests
    {
        private const string Root = "https://example.com/";

        private sealed class TextHandler : HttpMessageHandler
        {
            private readonly Dictionary<string, (HttpStatusCode Status, string Body)> _responses = new(StringComparer.OrdinalIgnoreCase);

            public TextHandler Add(string url, string body, HttpStatusCode status = HttpStatusCode.OK)
            {
                _responses[url] = (status, body);
                return this;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (!_responses.TryGetValue(request.RequestUri!.AbsoluteUri, out var response))
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") });

                return Task.FromResult(new HttpResponseMessage(response.Status)
                {
                    Content = new StringContent(response.Body)
                });
            }
        }

        private static string Sitemap(params string[] urls)
            => "<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" +
               string.Concat(urls.Select(u => $"<url><loc>{u}</loc></url>")) + "</urlset>";

        [Test]
        public async Task Discover_ReadsRobotsAndTheConventionalSitemap()
        {
            var handler = new TextHandler()
                .Add("https://example.com/robots.txt", "User-agent: *\nDisallow: /admin")
                .Add("https://example.com/sitemap.xml", Sitemap("https://example.com/a", "https://example.com/b"));

            var provider = new HttpSiteEntryPointProvider(new HttpClient(handler));

            var result = await provider.DiscoverAsync(new Uri(Root), new CrawlOptions(), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Robots.IsAllowed("any", "/admin"), Is.False);
                Assert.That(result.SitemapUrls.Select(it => it.AbsoluteUri),
                    Is.EqualTo(new[] { "https://example.com/a", "https://example.com/b" }));
            });
        }

        [Test]
        public async Task Discover_PrefersTheSitemapsDeclaredInRobots()
        {
            var handler = new TextHandler()
                .Add("https://example.com/robots.txt", "Sitemap: https://example.com/custom.xml")
                .Add("https://example.com/custom.xml", Sitemap("https://example.com/from-robots"));

            var provider = new HttpSiteEntryPointProvider(new HttpClient(handler));

            var result = await provider.DiscoverAsync(new Uri(Root), new CrawlOptions(), CancellationToken.None);

            Assert.That(result.SitemapUrls.Single().AbsoluteUri, Is.EqualTo("https://example.com/from-robots"));
        }

        [Test]
        public async Task Discover_FollowsSitemapIndexes()
        {
            var handler = new TextHandler()
                .Add("https://example.com/robots.txt", "")
                .Add("https://example.com/sitemap.xml",
                    "<?xml version=\"1.0\"?><sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" +
                    "<sitemap><loc>https://example.com/pages.xml</loc></sitemap></sitemapindex>")
                .Add("https://example.com/pages.xml", Sitemap("https://example.com/deep"));

            var provider = new HttpSiteEntryPointProvider(new HttpClient(handler));

            var result = await provider.DiscoverAsync(new Uri(Root), new CrawlOptions(), CancellationToken.None);

            Assert.That(result.SitemapUrls.Single().AbsoluteUri, Is.EqualTo("https://example.com/deep"));
        }

        [Test]
        public async Task Discover_TreatsAMissingRobotsAsNoRestrictions()
        {
            var provider = new HttpSiteEntryPointProvider(new HttpClient(new TextHandler()));

            var result = await provider.DiscoverAsync(new Uri(Root), new CrawlOptions(), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Robots.IsAllowed("any", "/anything"), Is.True);
                Assert.That(result.SitemapUrls, Is.Empty);
            });
        }

        [Test]
        public async Task Discover_DoesNotFailOnAMalformedSitemap()
        {
            //A broken sitemap is a finding for a check to report, not a reason to abandon the crawl.
            var handler = new TextHandler()
                .Add("https://example.com/robots.txt", "")
                .Add("https://example.com/sitemap.xml", "this is not xml at all");

            var provider = new HttpSiteEntryPointProvider(new HttpClient(handler));

            var result = await provider.DiscoverAsync(new Uri(Root), new CrawlOptions(), CancellationToken.None);

            Assert.That(result.SitemapUrls, Is.Empty);
        }

        [Test]
        public async Task Discover_SkipsSitemapsWhenSeedingFromThemIsOff()
        {
            var handler = new TextHandler()
                .Add("https://example.com/robots.txt", "")
                .Add("https://example.com/sitemap.xml", Sitemap("https://example.com/a"));

            var provider = new HttpSiteEntryPointProvider(new HttpClient(handler));

            var result = await provider.DiscoverAsync(new Uri(Root),
                new CrawlOptions { SeedFromSitemap = false }, CancellationToken.None);

            Assert.That(result.SitemapUrls, Is.Empty);
        }
    }
}
