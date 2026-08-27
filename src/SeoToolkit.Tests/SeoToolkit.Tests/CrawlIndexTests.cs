using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class CrawlIndexTests
    {
        [Test]
        public void TryGet_FindsARecordedResourceByItsNormalisedUrl()
        {
            var index = new CrawlIndex();
            SeoCheckApiTests.AddPage(index, "https://example.com/a", title: "A");

            Assert.Multiple(() =>
            {
                Assert.That(index.TryGet("https://example.com/a", out var found), Is.True);
                Assert.That(found!.Title, Is.EqualTo("A"));
                Assert.That(index.TryGet("https://example.com/missing", out _), Is.False);
            });
        }

        [Test]
        public void GetOrCreateId_GivesLinkedButUncrawledUrlsAnIdentity()
        {
            //A link to a page that was never reached is exactly what broken link and orphan
            //analysis is about, so such urls still need a node in the graph.
            var index = new CrawlIndex();

            var first = index.GetOrCreateId("https://example.com/never-crawled");
            var second = index.GetOrCreateId("https://example.com/never-crawled");

            Assert.Multiple(() =>
            {
                Assert.That(first, Is.EqualTo(second));
                Assert.That(index.TryGetById(first, out _), Is.False, "no summary, because it was never fetched");
            });
        }

        [Test]
        public void Outlinks_AreDeduplicated()
        {
            var index = new CrawlIndex();
            var a = SeoCheckApiTests.AddPage(index, "https://example.com/a", outlinks: new[]
            {
                "https://example.com/b",
                "https://example.com/b",
                "https://example.com/c"
            });

            Assert.That(index.OutlinksFrom(a), Has.Count.EqualTo(2),
                "a page linking to the same target repeatedly is still one edge");
        }

        [Test]
        public void InlinksTo_ReversesTheLinkGraph()
        {
            var index = new CrawlIndex();
            var a = SeoCheckApiTests.AddPage(index, "https://example.com/a", outlinks: new[] { "https://example.com/target" });
            var b = SeoCheckApiTests.AddPage(index, "https://example.com/b", outlinks: new[] { "https://example.com/target" });
            var target = SeoCheckApiTests.AddPage(index, "https://example.com/target");

            //The target was linked to before it was crawled, so it must keep the id assigned then.
            Assert.That(index.InlinksTo(target), Is.EquivalentTo(new[] { a, b }));
        }

        [Test]
        public void InlinksTo_ReturnsEmptyForAnOrphan()
        {
            var index = new CrawlIndex();
            var orphan = SeoCheckApiTests.AddPage(index, "https://example.com/orphan");

            Assert.That(index.InlinksTo(orphan), Is.Empty);
        }

        [Test]
        public void DuplicatesBy_GroupsPagesSharingAValue_AndIgnoresUniqueOnes()
        {
            var index = new CrawlIndex();
            SeoCheckApiTests.AddPage(index, "https://example.com/a", title: "Same");
            SeoCheckApiTests.AddPage(index, "https://example.com/b", title: "Same");
            SeoCheckApiTests.AddPage(index, "https://example.com/c", title: "Unique");

            var groups = index.DuplicatesBy(DuplicateFacet.Title).ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(groups, Has.Length.EqualTo(1));
                Assert.That(groups[0], Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void DuplicatesBy_IgnoresPagesWithNoValue()
        {
            var index = new CrawlIndex();
            SeoCheckApiTests.AddPage(index, "https://example.com/a", title: null);
            SeoCheckApiTests.AddPage(index, "https://example.com/b", title: null);

            //Two pages both missing a title is a missing-title finding, not a duplicate one.
            Assert.That(index.DuplicatesBy(DuplicateFacet.Title), Is.Empty);
        }

        [Test]
        public void DuplicatesBy_ExcludesErrorPagesExternalPagesAndNonPages()
        {
            var index = new CrawlIndex();
            SeoCheckApiTests.AddPage(index, "https://example.com/ok", title: "Same");
            SeoCheckApiTests.AddPage(index, "https://example.com/404", title: "Same", statusCode: 404);
            SeoCheckApiTests.AddPage(index, "https://other.com/ext", title: "Same", isInternal: false);
            SeoCheckApiTests.AddPage(index, "https://example.com/img", title: "Same", kind: ResourceKind.Image);

            //Two 404s sharing a title is not a duplicate content problem, and reporting it as
            //one would bury the real findings.
            Assert.That(index.DuplicatesBy(DuplicateFacet.Title), Is.Empty);
        }

        [Test]
        public void DuplicatesBy_MatchesBodyContentOnHash_SinceTextIsNeverRetained()
        {
            var index = new CrawlIndex();
            var a = SeoCheckApiTests.AddPage(index, "https://example.com/a", contentHash: 12345);
            var b = SeoCheckApiTests.AddPage(index, "https://example.com/b", contentHash: 12345);
            SeoCheckApiTests.AddPage(index, "https://example.com/c", contentHash: 999);

            var groups = index.DuplicatesBy(DuplicateFacet.Content).ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(groups, Has.Length.EqualTo(1));
                Assert.That(groups[0].Select(it => it.Id), Is.EquivalentTo(new[] { a, b }));
            });
        }

        [Test]
        public void DuplicatesBy_SharesOneGroupingPassAcrossRepeatedCalls()
        {
            //Four duplicate checks asking the same question must not each walk the corpus.
            var index = new CrawlIndex();
            SeoCheckApiTests.AddPage(index, "https://example.com/a", title: "Same");
            SeoCheckApiTests.AddPage(index, "https://example.com/b", title: "Same");

            var first = index.DuplicatesBy(DuplicateFacet.Title);
            var second = index.DuplicatesBy(DuplicateFacet.Title);

            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void DuplicatesBy_DistinguishesFacets()
        {
            var index = new CrawlIndex();
            SeoCheckApiTests.AddPage(index, "https://example.com/a", title: "Same", description: "DescA");
            SeoCheckApiTests.AddPage(index, "https://example.com/b", title: "Same", description: "DescB");

            Assert.Multiple(() =>
            {
                Assert.That(index.DuplicatesBy(DuplicateFacet.Title).Count(), Is.EqualTo(1));
                Assert.That(index.DuplicatesBy(DuplicateFacet.MetaDescription), Is.Empty);
            });
        }

        [Test]
        public void SitemapAndUmbracoUrls_AreTrackedSeparatelyFromCrawledResources()
        {
            var index = new CrawlIndex();
            index.AddSitemapUrls(new[] { "https://example.com/a", "https://example.com/b" });
            index.AddUmbracoUrls(new[] { "https://example.com/a", "https://example.com/hidden" });
            SeoCheckApiTests.AddPage(index, "https://example.com/a");

            //The set difference is what orphan and sitemap-coverage checks are built on.
            var notCrawled = index.UmbracoUrls.Where(it => !index.TryGet(it, out _)).ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(index.SitemapUrls, Has.Count.EqualTo(2));
                Assert.That(notCrawled, Is.EqualTo(new[] { "https://example.com/hidden" }));
            });
        }

        [Test]
        public void IsAllowedByRobots_DefaultsToAllowingEverything()
        {
            Assert.That(new CrawlIndex().IsAllowedByRobots(new Uri("https://example.com/anything")), Is.True);
        }

        [Test]
        public void IsAllowedByRobots_DelegatesToTheSuppliedMatcher()
        {
            var index = new CrawlIndex(url => !url.AbsolutePath.StartsWith("/private"));

            Assert.Multiple(() =>
            {
                Assert.That(index.IsAllowedByRobots(new Uri("https://example.com/public")), Is.True);
                Assert.That(index.IsAllowedByRobots(new Uri("https://example.com/private/x")), Is.False);
            });
        }

        [Test]
        public void Add_IsSafeFromSeveralThreadsAtOnce()
        {
            //Resources are recorded from the crawler's worker threads.
            var index = new CrawlIndex();

            Parallel.For(0, 500, i =>
                SeoCheckApiTests.AddPage(index, $"https://example.com/page-{i}", title: $"Title {i % 10}"));

            Assert.Multiple(() =>
            {
                Assert.That(index.Count, Is.EqualTo(500));
                Assert.That(index.DuplicatesBy(DuplicateFacet.Title).Count(), Is.EqualTo(10));
            });
        }
    }
}
