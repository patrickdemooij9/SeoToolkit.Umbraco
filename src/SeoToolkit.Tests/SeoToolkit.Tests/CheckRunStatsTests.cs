using System.Net.Http;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;

namespace SeoToolkit.Tests
{
    /// <summary>
    /// The per-check totals a run stores. These matter more than they look: ApplicableCount is
    /// the denominator the health score divides by, so getting it wrong moves the score for
    /// reasons that have nothing to do with the site.
    /// </summary>
    [TestFixture]
    public class CheckRunStatsTests
    {
        private const string Root = "https://example.com/";

        private static async Task<CrawlResult> Crawl(FakeSiteHandler site,
            IReadOnlyList<ISeoCheck> checks,
            SeoCheckCapabilities available = SeoCheckCapabilities.CrawlIndex,
            FakeRunContext? run = null)
        {
            var options = new CrawlOptions { MaxConcurrency = 4, UserAgent = "SeoToolkit-Tests" };
            var engine = new CrawlEngine(
                new HttpResourceFetcher(new HttpClient(site), options),
                new DefaultUrlNormalizer(options),
                new AngleSharpPageFactsParser(),
                new HostPolitenessGate(TimeSpan.Zero));

            return await engine.CrawlAsync(new Uri(Root), options, checks,
                run ?? new FakeRunContext { AvailableCapabilities = available });
        }

        private static string Page(string title, params string[] links)
            => $"<html><head><title>{title}</title></head><body>" +
               string.Concat(links.Select(l => $"<a href=\"{l}\">l</a>")) +
               "</body></html>";

        [Test]
        public async Task ApplicableCount_CountsResourcesTheCheckActuallyLookedAt()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Page("A perfectly reasonable title", "/a", "/b"))
                .Page("https://example.com/a", Page("Another reasonable title"))
                .Page("https://example.com/b", Page("A third reasonable title"));

            var result = await Crawl(site, new ISeoCheck[] { new SampleTitleCheck() });

            var stats = result.CheckStats.Single(it => it.Alias == SampleTitleCheck.CheckAlias);

            Assert.Multiple(() =>
            {
                Assert.That(stats.ApplicableCount, Is.EqualTo(3));
                Assert.That(stats.FailedCount, Is.Zero);
                Assert.That(stats.DidRun, Is.True);
            });
        }

        [Test]
        public async Task FailedCount_CountsResourcesWithAProblem_NotTotalFindings()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Page("short", "/a"))
                .Page("https://example.com/a", Page("A perfectly reasonable title"));

            var result = await Crawl(site, new ISeoCheck[] { new SampleTitleCheck() });

            var stats = result.CheckStats.Single(it => it.Alias == SampleTitleCheck.CheckAlias);

            Assert.Multiple(() =>
            {
                Assert.That(stats.ApplicableCount, Is.EqualTo(2));
                Assert.That(stats.FailedCount, Is.EqualTo(1), "only the root has too short a title");
                Assert.That(stats.IssueCount, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task ApplicableCount_ExcludesResourcesTheCheckDoesNotApplyTo()
        {
            //An image is not something a title check applies to, so it must not inflate the
            //denominator and make the site look better than it is.
            var site = new FakeSiteHandler()
                .Page(Root, Page("A perfectly reasonable title", "/logo.png"))
                .Asset("https://example.com/logo.png", "image/png");

            var options = new CrawlOptions { MaxConcurrency = 2, CrawlAssets = true, UserAgent = "SeoToolkit-Tests" };
            var engine = new CrawlEngine(
                new HttpResourceFetcher(new HttpClient(site), options),
                new DefaultUrlNormalizer(options),
                new AngleSharpPageFactsParser(),
                new HostPolitenessGate(TimeSpan.Zero));

            var result = await engine.CrawlAsync(new Uri(Root), options,
                new ISeoCheck[] { new SampleTitleCheck() },
                new FakeRunContext { AvailableCapabilities = SeoCheckCapabilities.CrawlIndex });

            var stats = result.CheckStats.Single(it => it.Alias == SampleTitleCheck.CheckAlias);

            Assert.Multiple(() =>
            {
                Assert.That(result.Crawled, Is.EqualTo(2), "both the page and the image were fetched");
                Assert.That(stats.ApplicableCount, Is.EqualTo(1), "but only the page was checked");
            });
        }

        [Test]
        public async Task ASkippedCheck_IsStillReported_ButMarkedAsNotHavingRun()
        {
            //Dropping it from the summary entirely would silently change the score's denominator.
            var site = new FakeSiteHandler().Page(Root, Page("A perfectly reasonable title"));

            var result = await Crawl(site, new ISeoCheck[] { new SampleTitleCheck(), new NeedsRenderingCheck() });

            var skipped = result.CheckStats.Single(it => it.Alias == NeedsRenderingCheck.CheckAlias);

            Assert.Multiple(() =>
            {
                Assert.That(skipped.DidRun, Is.False);
                Assert.That(skipped.ApplicableCount, Is.Zero);
                Assert.That(skipped.FailedCount, Is.Zero);
            });
        }

        [Test]
        public async Task SiteWideFindings_AreCountedAgainstTheCheckThatRaisedThem()
        {
            //They are raised after the crawl rather than per resource, so they need counting
            //separately - otherwise a site check always looks like it found nothing.
            var site = new FakeSiteHandler()
                .Page(Root, Page("Shared title", "/a"))
                .Page("https://example.com/a", Page("Shared title"));

            var result = await Crawl(site, new ISeoCheck[] { new SampleDuplicateTitleCheck() });

            var stats = result.CheckStats.Single(it => it.Alias == SampleDuplicateTitleCheck.CheckAlias);

            Assert.Multiple(() =>
            {
                Assert.That(stats.IssueCount, Is.EqualTo(2));
                Assert.That(stats.FailedCount, Is.EqualTo(2), "two distinct pages share the title");
                Assert.That(stats.ApplicableCount, Is.EqualTo(2),
                    "a site check with no collector applies to the whole crawl");
            });
        }

        [Test]
        public async Task ACheckThatFoundNothing_StillRecordsThatItRan()
        {
            var site = new FakeSiteHandler().Page(Root, Page("A perfectly reasonable title"));

            var result = await Crawl(site, new ISeoCheck[] { new SampleDuplicateTitleCheck() });

            var stats = result.CheckStats.Single(it => it.Alias == SampleDuplicateTitleCheck.CheckAlias);

            Assert.Multiple(() =>
            {
                Assert.That(stats.DidRun, Is.True);
                Assert.That(stats.FailedCount, Is.Zero);
            });
        }

        [Test]
        public async Task CollectorBasedSiteChecks_AccumulateApplicableCountPerResource()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Page("A perfectly reasonable title", "/a"))
                .Page("https://example.com/a", Page("Another reasonable title"));

            var result = await Crawl(site, new ISeoCheck[] { new SampleSlowPageCheck() });

            var stats = result.CheckStats.Single(it => it.Alias == SampleSlowPageCheck.CheckAlias);

            Assert.That(stats.ApplicableCount, Is.EqualTo(2));
        }

        [Test]
        public async Task EveryRegisteredCheck_AppearsInTheStats()
        {
            var site = new FakeSiteHandler().Page(Root, Page("A perfectly reasonable title"));

            var result = await Crawl(site,
                new ISeoCheck[] { new SampleTitleCheck(), new SampleDuplicateTitleCheck(), new SampleSlowPageCheck() });

            Assert.That(result.CheckStats.Select(it => it.Alias), Is.EquivalentTo(new[]
            {
                SampleTitleCheck.CheckAlias,
                SampleDuplicateTitleCheck.CheckAlias,
                SampleSlowPageCheck.CheckAlias
            }));
        }

        private sealed class NeedsRenderingCheck : SeoPageCheckBase
        {
            public const string CheckAlias = "Test.NeedsRendering";

            protected override SeoCheckDescriptor CreateDescriptor() => new()
            {
                Alias = CheckAlias,
                Name = "Needs a browser",
                Category = SeoCheckCategory.Performance,
                RequiredCapabilities = SeoCheckCapabilities.RenderedDom
            };

            protected override void Check(in SeoPageCheckContext context) => context.Report("ShouldNotRun");
        }
    }
}
