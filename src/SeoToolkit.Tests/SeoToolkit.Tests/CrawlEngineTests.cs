using System.Net;
using System.Net.Http;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    /// <summary>Serves a fake site so the crawler can be exercised without a network.</summary>
    internal sealed class FakeSiteHandler : HttpMessageHandler
    {
        private sealed record Response(HttpStatusCode Status, string? Body, string ContentType, string? Location);

        private readonly Dictionary<string, Response> _responses = new(StringComparer.OrdinalIgnoreCase);

        public int RequestCount { get; private set; }
        public List<string> RequestedUrls { get; } = new();

        public FakeSiteHandler Page(string url, string body)
        {
            _responses[url] = new Response(HttpStatusCode.OK, body, "text/html; charset=utf-8", null);
            return this;
        }

        public FakeSiteHandler Status(string url, HttpStatusCode status)
        {
            _responses[url] = new Response(status, "<html><body>error</body></html>", "text/html", null);
            return this;
        }

        public FakeSiteHandler Redirect(string url, string to, HttpStatusCode status = HttpStatusCode.MovedPermanently)
        {
            _responses[url] = new Response(status, null, "text/html", to);
            return this;
        }

        public FakeSiteHandler Asset(string url, string contentType)
        {
            _responses[url] = new Response(HttpStatusCode.OK, "binary-ish", contentType, null);
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (RequestedUrls)
            {
                RequestCount++;
                RequestedUrls.Add(request.RequestUri!.AbsoluteUri);
            }

            if (!_responses.TryGetValue(request.RequestUri!.AbsoluteUri, out var response))
                response = new Response(HttpStatusCode.NotFound, "<html><body>not found</body></html>", "text/html", null);

            var message = new HttpResponseMessage(response.Status)
            {
                Content = new StringContent(response.Body ?? string.Empty)
            };
            message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                response.ContentType.Split(';')[0]);

            if (response.Location is not null)
                message.Headers.Location = new Uri(response.Location);

            return Task.FromResult(message);
        }
    }

    [TestFixture]
    public class CrawlEngineTests
    {
        private const string Root = "https://example.com/";

        private static (CrawlEngine Engine, FakeSiteHandler Site) Build(CrawlOptions options, FakeSiteHandler site)
        {
            var client = new HttpClient(site);
            var normalizer = new DefaultUrlNormalizer(options);
            var fetcher = new HttpResourceFetcher(client, options);
            var politeness = new HostPolitenessGate(TimeSpan.Zero);

            return (new CrawlEngine(fetcher, normalizer, new AngleSharpPageFactsParser(), politeness), site);
        }

        private static CrawlOptions Options(Action<CrawlOptionsBuilder>? configure = null)
        {
            var builder = new CrawlOptionsBuilder();
            configure?.Invoke(builder);
            return builder.Build();
        }

        internal sealed class CrawlOptionsBuilder
        {
            public int? MaxPages { get; set; }
            public int? MaxDepth { get; set; }
            public int MaxConcurrency { get; set; } = 4;
            public bool RespectRobots { get; set; } = true;
            public bool CrawlAssets { get; set; }

            public CrawlOptions Build() => new()
            {
                MaxPages = MaxPages,
                MaxDepth = MaxDepth,
                MaxConcurrency = MaxConcurrency,
                RespectRobotsTxt = RespectRobots,
                CrawlAssets = CrawlAssets,
                DelayBetweenRequestsMs = 0,
                UserAgent = "SeoToolkit-Tests"
            };
        }

        private static Task<CrawlResult> Crawl(CrawlEngine engine, CrawlOptions options,
            IReadOnlyList<ISeoCheck>? checks = null,
            RobotsTxtMatcher? robots = null,
            SeoCheckCapabilities available = SeoCheckCapabilities.CrawlIndex,
            CancellationToken cancellationToken = default)
            => engine.CrawlAsync(new Uri(Root), options,
                checks ?? Array.Empty<ISeoCheck>(),
                new FakeRunContext { AvailableCapabilities = available },
                robots,
                null,
                cancellationToken);

        private static string Links(params string[] hrefs)
            => "<html><head><title>Page</title></head><body>" +
               string.Concat(hrefs.Select(h => $"<a href=\"{h}\">link</a>")) +
               "</body></html>";

        [Test]
        public async Task Crawl_FollowsInternalLinks()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/a", "/b"))
                .Page("https://example.com/a", Links("/c"))
                .Page("https://example.com/b", Links())
                .Page("https://example.com/c", Links());

            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options);

            Assert.That(result.Crawled, Is.EqualTo(4));
        }

        [Test]
        public async Task Crawl_VisitsEachPageOnlyOnce_EvenWhenManyPagesLinkToIt()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/a", "/b"))
                .Page("https://example.com/a", Links("/shared"))
                .Page("https://example.com/b", Links("/shared"))
                .Page("https://example.com/shared", Links());

            var options = Options();
            var (engine, handler) = Build(options, site);

            await Crawl(engine, options);

            Assert.That(handler.RequestedUrls.Count(it => it.EndsWith("/shared")), Is.EqualTo(1));
        }

        [Test]
        public async Task Crawl_TreatsUrlsDifferingOnlyByFragmentOrTrackingAsOnePage()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/a", "/a#section", "/a?utm_source=x"))
                .Page("https://example.com/a", Links());

            var options = Options();
            var (engine, handler) = Build(options, site);

            var result = await Crawl(engine, options);

            Assert.Multiple(() =>
            {
                Assert.That(result.Crawled, Is.EqualTo(2));
                Assert.That(handler.RequestedUrls.Count(it => it.Contains("/a")), Is.EqualTo(1));
            });
        }

        [Test]
        public async Task Crawl_RecordsExternalLinksAsEdgesWithoutFollowingThem()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("https://external.com/x"))
                .Page("https://external.com/x", Links());

            var options = Options();
            var (engine, handler) = Build(options, site);

            var result = await Crawl(engine, options);

            Assert.Multiple(() =>
            {
                Assert.That(result.Crawled, Is.EqualTo(1), "the external page is not fetched");
                Assert.That(handler.RequestedUrls, Has.None.Contains("external.com"));

                var rootId = ((CrawlIndex)result.Index).GetOrCreateId("https://example.com/");
                Assert.That(result.Index.OutlinksFrom(rootId), Has.Count.EqualTo(1),
                    "but the link is still an edge in the graph");
            });
        }

        [Test]
        public async Task Crawl_StopsAtMaxPages()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/a", "/b", "/c", "/d", "/e"));
            foreach (var path in new[] { "a", "b", "c", "d", "e" })
                site.Page($"https://example.com/{path}", Links());

            var options = Options(it => it.MaxPages = 3);
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options);

            Assert.That(result.Crawled, Is.EqualTo(3));
        }

        [Test]
        public async Task Crawl_StopsAtMaxDepth()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/depth1"))
                .Page("https://example.com/depth1", Links("/depth2"))
                .Page("https://example.com/depth2", Links("/depth3"))
                .Page("https://example.com/depth3", Links());

            var options = Options(it => it.MaxDepth = 1);
            var (engine, handler) = Build(options, site);

            var result = await Crawl(engine, options);

            Assert.Multiple(() =>
            {
                Assert.That(result.Crawled, Is.EqualTo(2), "the seed plus one level");
                Assert.That(handler.RequestedUrls, Has.None.Contains("depth2"));
            });
        }

        [Test]
        public async Task Crawl_CapturesTheRedirectChainRatherThanHidingIt()
        {
            var site = new FakeSiteHandler()
                .Redirect(Root, "https://example.com/step", HttpStatusCode.Found)
                .Redirect("https://example.com/step", "https://example.com/final")
                .Page("https://example.com/final", Links());

            var options = Options();
            var (engine, _) = Build(options, site);

            CrawledResource? seed = null;
            engine.ResourceCrawled += (_, e) =>
            {
                if (e.Resource.RequestedUrl.AbsoluteUri == Root) seed = e.Resource;
            };

            await Crawl(engine, options);

            Assert.Multiple(() =>
            {
                Assert.That(seed, Is.Not.Null);
                Assert.That(seed!.RedirectChain, Has.Count.EqualTo(2));
                Assert.That(seed.RedirectChain[0].IsPermanent, Is.False, "302");
                Assert.That(seed.RedirectChain[1].IsPermanent, Is.True, "301");
                Assert.That(seed.FinalUrl.AbsoluteUri, Is.EqualTo("https://example.com/final"));
                Assert.That(seed.StatusCode, Is.EqualTo(200));
            });
        }

        [Test]
        public async Task Crawl_DetectsARedirectLoopInsteadOfSpinning()
        {
            var site = new FakeSiteHandler()
                .Redirect(Root, "https://example.com/loop")
                .Redirect("https://example.com/loop", Root);

            var options = Options();
            var (engine, _) = Build(options, site);

            CrawledResource? seed = null;
            engine.ResourceCrawled += (_, e) => seed ??= e.Resource;

            await Crawl(engine, options);

            Assert.Multiple(() =>
            {
                Assert.That(seed!.Failure, Is.EqualTo(CrawlFailureReason.RedirectLoop));
                Assert.That(seed.IsRedirectLoop, Is.True);
            });
        }

        [Test]
        public async Task Crawl_RecordsErrorStatusesRatherThanTreatingThemAsFailures()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/missing", "/broken"))
                .Status("https://example.com/missing", HttpStatusCode.NotFound)
                .Status("https://example.com/broken", HttpStatusCode.InternalServerError);

            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options);

            Assert.Multiple(() =>
            {
                Assert.That(result.Index.TryGet("https://example.com/missing", out var missing), Is.True);
                Assert.That(missing!.StatusCode, Is.EqualTo(404));
                Assert.That(missing.Indexability.HasFlag(IndexabilityFlags.ErrorStatus), Is.True);

                Assert.That(result.Index.TryGet("https://example.com/broken", out var broken), Is.True);
                Assert.That(broken!.StatusCode, Is.EqualTo(500));
            });
        }

        [Test]
        public async Task Crawl_ObeysRobotsTxtByDefault()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/public", "/private/secret"))
                .Page("https://example.com/public", Links())
                .Page("https://example.com/private/secret", Links());

            var robots = RobotsTxtMatcher.Parse("User-agent: *\nDisallow: /private");
            var options = Options();
            var (engine, handler) = Build(options, site);

            var result = await Crawl(engine, options, robots: robots);

            Assert.Multiple(() =>
            {
                Assert.That(result.Crawled, Is.EqualTo(2));
                Assert.That(handler.RequestedUrls, Has.None.Contains("private"));
            });
        }

        [Test]
        public async Task Crawl_CanBeToldToIgnoreRobotsTxt()
        {
            //An auditor often needs to see precisely the pages that are being blocked.
            var site = new FakeSiteHandler()
                .Page(Root, Links("/private/secret"))
                .Page("https://example.com/private/secret", Links());

            var robots = RobotsTxtMatcher.Parse("User-agent: *\nDisallow: /private");
            var options = Options(it => it.RespectRobots = false);
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options, robots: robots);

            Assert.That(result.Crawled, Is.EqualTo(2));
        }

        [Test]
        public async Task Crawl_RunsPageChecksAndCollectsTheirIssues()
        {
            var site = new FakeSiteHandler()
                .Page(Root, "<html><head><title>" + new string('x', 90) + "</title></head><body></body></html>");

            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options, new ISeoCheck[] { new SampleTitleCheck() });

            Assert.Multiple(() =>
            {
                Assert.That(result.Issues, Has.Count.EqualTo(1));
                Assert.That(result.Issues[0].Variant, Is.EqualTo("TooLong"));
            });
        }

        [Test]
        public async Task Crawl_RunsSiteChecksOnceTheCrawlHasFinished()
        {
            const string sharedTitle = "<html><head><title>Shared</title></head><body>{0}</body></html>";

            var site = new FakeSiteHandler()
                .Page(Root, string.Format(sharedTitle, "<a href=\"/a\">l</a>"))
                .Page("https://example.com/a", string.Format(sharedTitle, ""));

            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options, new ISeoCheck[] { new SampleDuplicateTitleCheck() });

            //Both pages share a title - a finding no per-page check could ever make.
            Assert.That(result.Issues.Where(it => it.CheckAlias == SampleDuplicateTitleCheck.CheckAlias),
                Has.Exactly(2).Items);
        }

        [Test]
        public async Task Crawl_DoesNotRetainBodiesWhenNoCheckAskedForThem()
        {
            var site = new FakeSiteHandler().Page(Root, Links());

            var options = Options();
            var (engine, _) = Build(options, site);

            CrawledResource? resource = null;
            engine.ResourceCrawled += (_, e) => resource = e.Resource;

            await Crawl(engine, options, new ISeoCheck[] { new SampleTitleCheck() });

            Assert.Multiple(() =>
            {
                Assert.That(resource!.RawBody, Is.Null, "nothing declared RawBody");
                Assert.That(resource.Facts, Is.Not.Null, "but the page was still parsed");
            });
        }

        [Test]
        public async Task Crawl_RetainsBodiesWhenACheckDeclaresThatItNeedsThem()
        {
            var site = new FakeSiteHandler().Page(Root, Links());

            var options = Options();
            var (engine, _) = Build(options, site);

            CrawledResource? resource = null;
            engine.ResourceCrawled += (_, e) => resource = e.Resource;

            await Crawl(engine, options,
                new ISeoCheck[] { new RawBodyCheck() },
                available: SeoCheckCapabilities.CrawlIndex | SeoCheckCapabilities.RawBody);

            Assert.That(resource!.RawBody, Is.Not.Null);
        }

        [Test]
        public async Task Crawl_SkipsAChecksWhoseCapabilitiesTheRunCannotMeet()
        {
            var site = new FakeSiteHandler().Page(Root, Links());

            var options = Options();
            var (engine, _) = Build(options, site);

            //No renderer is registered, so the check is not run at all rather than silently passing.
            var result = await Crawl(engine, options,
                new ISeoCheck[] { new RenderedDomCheck() },
                available: SeoCheckCapabilities.CrawlIndex);

            Assert.That(result.Issues, Is.Empty);
        }

        [Test]
        public async Task Crawl_SkipsAChecksWhoseFeatureIsNotEnabled()
        {
            var site = new FakeSiteHandler().Page(Root, Links());
            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await engine.CrawlAsync(new Uri(Root), options,
                new ISeoCheck[] { new PremiumCheck() },
                new FakeRunContext { AvailableCapabilities = SeoCheckCapabilities.CrawlIndex });

            Assert.That(result.Issues, Is.Empty);
        }

        [Test]
        public async Task Crawl_RunsAChecksWhoseFeatureIsEnabled()
        {
            var site = new FakeSiteHandler().Page(Root, Links());
            var options = Options();
            var (engine, _) = Build(options, site);

            var run = new FakeRunContext { AvailableCapabilities = SeoCheckCapabilities.CrawlIndex };
            run.Features.Add(PremiumCheck.Feature);

            var result = await engine.CrawlAsync(new Uri(Root), options,
                new ISeoCheck[] { new PremiumCheck() }, run);

            Assert.That(result.Issues, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Crawl_SkipsChecksThatDoNotApplyToTheResourceKind()
        {
            var site = new FakeSiteHandler()
                .Page(Root, "<html><head><title>A perfectly fine page title</title></head>" +
                            "<body><a href=\"/logo.png\">img</a></body></html>")
                .Asset("https://example.com/logo.png", "image/png");

            var options = Options(it => it.CrawlAssets = true);
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options, new ISeoCheck[] { new SampleTitleCheck() });

            //The image is crawled, but a title check must not fire against it - an image having
            //no <title> is not a finding.
            Assert.Multiple(() =>
            {
                Assert.That(result.Crawled, Is.EqualTo(2));
                Assert.That(result.Index.TryGet("https://example.com/logo.png", out var image), Is.True);
                Assert.That(image!.Kind, Is.EqualTo(ResourceKind.Image));
                Assert.That(result.Issues, Is.Empty);
            });
        }

        [Test]
        public async Task Crawl_SurvivesACheckThatThrows()
        {
            //A misbehaving check, quite possibly from a third party, must not abort the crawl.
            var site = new FakeSiteHandler()
                .Page(Root, Links("/a"))
                .Page("https://example.com/a", Links());

            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options, new ISeoCheck[] { new ThrowingCheck(), new SampleTitleCheck() });

            Assert.That(result.Crawled, Is.EqualTo(2));
        }

        [Test]
        public async Task Crawl_TerminatesOnALargeSiteWithSeveralWorkers()
        {
            //Completion detection is the subtlest part of a parallel crawl: the queue can be
            //momentarily empty while workers are still about to schedule more.
            var site = new FakeSiteHandler();
            const int pages = 200;

            site.Page(Root, Links(Enumerable.Range(0, pages).Select(i => $"/p{i}").ToArray()));
            for (var i = 0; i < pages; i++)
            {
                var next = i + 1 < pages ? $"/p{i + 1}" : "/";
                site.Page($"https://example.com/p{i}", Links(next));
            }

            var options = Options(it => it.MaxConcurrency = 8);
            var (engine, _) = Build(options, site);

            var crawl = Crawl(engine, options);
            var finished = await Task.WhenAny(crawl, Task.Delay(TimeSpan.FromSeconds(30)));

            Assert.That(finished, Is.SameAs(crawl), "the crawl should finish, not hang");
            Assert.That((await crawl).Crawled, Is.EqualTo(pages + 1));
        }

        [Test]
        public async Task Crawl_StopsWhenCancelled()
        {
            var site = new FakeSiteHandler();
            site.Page(Root, Links(Enumerable.Range(0, 500).Select(i => $"/p{i}").ToArray()));
            for (var i = 0; i < 500; i++)
                site.Page($"https://example.com/p{i}", Links());

            var options = Options(it => it.MaxConcurrency = 2);
            var (engine, _) = Build(options, site);

            using var cts = new CancellationTokenSource();
            engine.ResourceCrawled += (_, e) =>
            {
                if (e.Completed >= 5) cts.Cancel();
            };

            var crawl = Crawl(engine, options, cancellationToken: cts.Token);
            var finished = await Task.WhenAny(crawl, Task.Delay(TimeSpan.FromSeconds(30)));

            Assert.That(finished, Is.SameAs(crawl), "cancellation should end the crawl promptly");

            var result = await crawl;
            Assert.Multiple(() =>
            {
                Assert.That(result.WasCancelled, Is.True);
                Assert.That(result.Crawled, Is.LessThan(501));
            });
        }

        [Test]
        public async Task Crawl_BuildsALinkGraphThatSiteChecksCanWalk()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Links("/a", "/b"))
                .Page("https://example.com/a", Links("/b"))
                .Page("https://example.com/b", Links())
                .Page("https://example.com/orphan", Links());

            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options);
            var index = (CrawlIndex)result.Index;

            var bId = index.GetOrCreateId("https://example.com/b");

            Assert.That(index.InlinksTo(bId), Has.Count.EqualTo(2), "linked from the root and from /a");
        }

        [Test]
        public async Task Crawl_MarksNoindexPagesAsNotIndexable()
        {
            var site = new FakeSiteHandler()
                .Page(Root, "<html><head><meta name=\"robots\" content=\"noindex\"></head><body></body></html>");

            var options = Options();
            var (engine, _) = Build(options, site);

            var result = await Crawl(engine, options);

            Assert.That(result.Index.TryGet("https://example.com/", out var summary), Is.True);
            Assert.That(summary!.Indexability.HasFlag(IndexabilityFlags.NoIndex), Is.True);
        }

        // ---- checks used only by these tests -------------------------------------------------

        private sealed class RawBodyCheck : SeoPageCheckBase
        {
            protected override SeoCheckDescriptor CreateDescriptor() => new()
            {
                Alias = "Test.RawBody",
                Name = "Needs raw body",
                Category = SeoCheckCategory.Other,
                RequiredCapabilities = SeoCheckCapabilities.RawBody
            };
        }

        private sealed class RenderedDomCheck : SeoPageCheckBase
        {
            protected override SeoCheckDescriptor CreateDescriptor() => new()
            {
                Alias = "Test.RenderedDom",
                Name = "Needs a browser",
                Category = SeoCheckCategory.Other,
                RequiredCapabilities = SeoCheckCapabilities.RenderedDom
            };

            protected override void Check(in SeoPageCheckContext context) => context.Report("ShouldNotRun");
        }

        private sealed class PremiumCheck : SeoPageCheckBase
        {
            public const string Feature = "SiteAudit.Premium";

            protected override SeoCheckDescriptor CreateDescriptor() => new()
            {
                Alias = "Test.Premium",
                Name = "Premium only",
                Category = SeoCheckCategory.Other,
                RequiresFeature = Feature
            };

            protected override void Check(in SeoPageCheckContext context) => context.Report("Ran");
        }

        private sealed class ThrowingCheck : SeoPageCheckBase
        {
            protected override SeoCheckDescriptor CreateDescriptor() => new()
            {
                Alias = "Test.Throws",
                Name = "Throws",
                Category = SeoCheckCategory.Other
            };

            protected override void Check(in SeoPageCheckContext context)
                => throw new InvalidOperationException("boom");
        }
    }
}
