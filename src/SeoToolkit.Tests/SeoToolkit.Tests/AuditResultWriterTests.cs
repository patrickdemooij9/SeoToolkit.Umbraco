using System.Data;
using Moq;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
// Both namespaces declare these, and the toolkit's own SeoToolkit.Umbraco namespace shadows
// the Umbraco root, so they are pinned explicitly.
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace SeoToolkit.Tests
{
    /// <summary>
    /// Records what was written, so the batching can be asserted on without a database.
    /// </summary>
    internal sealed class RecordingRunRepository : ISiteAuditRunRepository
    {
        public List<IReadOnlyList<AuditResourceWrite>> Batches { get; } = new();
        public List<AuditIssue> SiteIssues { get; } = new();
        public List<AuditCheckRun> CheckRuns { get; } = new();
        public List<AuditRun> SavedRuns { get; } = new();
        public List<int> DeletedIds { get; } = new();
        public int HeartbeatCount { get; private set; }

        public bool ClaimSucceeds { get; set; } = true;
        public int RetentionResult { get; set; }
        public int? RetentionKeep { get; private set; }
        public TimeSpan? StaleAfter { get; private set; }

        public IEnumerable<AuditResourceWrite> AllWrites => Batches.SelectMany(it => it);

        public void AddResults(int runId, IReadOnlyList<AuditResourceWrite> resources) => Batches.Add(resources);

        public void AddSiteIssues(int runId, IReadOnlyList<AuditIssue> issues) => SiteIssues.AddRange(issues);

        public void SaveCheckRuns(int runId, IReadOnlyList<AuditCheckRun> checkRuns) => CheckRuns.AddRange(checkRuns);

        public void UpdateHeartbeat(int runId, int totalCrawled, int totalDiscovered) => HeartbeatCount++;

        public AuditRun Save(AuditRun run)
        {
            if (run.Id == 0) run.Id = SavedRuns.Count + 1;
            SavedRuns.Add(run);
            return run;
        }

        public void Delete(int id) => DeletedIds.Add(id);

        public int DeleteAllButNewest(int keep)
        {
            RetentionKeep = keep;
            return RetentionResult;
        }

        public bool TryClaim(int runId, string claimedBy) => ClaimSucceeds;

        public int MarkStaleRunsInterrupted(TimeSpan staleAfter)
        {
            StaleAfter = staleAfter;
            return 0;
        }

        public IReadOnlyList<AuditRun> Queued { get; set; } = Array.Empty<AuditRun>();

        public IReadOnlyList<AuditRun> GetQueued(int max) => Queued.Take(max).ToArray();

        // Not exercised by these tests.
        public AuditRun? Get(int id) => null;
        public AuditRun? Get(Guid key) => null;
        public PagedModel<AuditRun> GetPaged(int skip, int take) => new(0, Array.Empty<AuditRun>());
        public IReadOnlyList<AuditCheckRun> GetCheckRuns(int runId) => CheckRuns;
        public PagedModel<AuditResource> GetResources(int runId, int skip, int take, AuditResourceFilter? filter = null)
            => new(0, Array.Empty<AuditResource>());
        public AuditResource? GetResource(int runId, int resourceId) => null;
        public PagedModel<AuditIssue> GetIssues(int runId, int skip, int take, AuditIssueFilter? filter = null)
            => new(0, Array.Empty<AuditIssue>());
        public IReadOnlyList<AuditIssue> GetIssuesForResource(int runId, int resourceId) => Array.Empty<AuditIssue>();
    }

    [TestFixture]
    public class AuditResultWriterTests
    {
        private static IScopeProvider ScopeProvider()
        {
            var provider = new Mock<IScopeProvider>();
            provider
                .Setup(x => x.CreateScope(
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<RepositoryCacheMode>(),
                    It.IsAny<IEventDispatcher>(),
                    It.IsAny<IScopedNotificationPublisher>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(() => Mock.Of<IScope>());

            return provider.Object;
        }

        private static (AuditResultWriter Writer, RecordingRunRepository Repository) Build(int batchSize)
        {
            var repository = new RecordingRunRepository();
            return (new AuditResultWriter(1, repository, ScopeProvider(), batchSize), repository);
        }

        private static ResourceCrawledEventArgs Crawled(string url, int completed,
            params (SeoSeverity Severity, string Alias)[] issues)
        {
            var uri = new Uri(url);
            var resource = new CrawledResource
            {
                RequestedUrl = uri,
                FinalUrl = uri,
                NormalizedUrl = uri.AbsoluteUri,
                UrlHash = "hash",
                StatusCode = 200,
                Kind = ResourceKind.HtmlPage,
                IsInternal = true
            };

            var summary = new ResourceSummary
            {
                Id = completed,
                NormalizedUrl = uri.AbsoluteUri,
                Url = uri,
                StatusCode = 200,
                Kind = ResourceKind.HtmlPage,
                IsInternal = true,
                Indexability = IndexabilityFlags.NoIndex
            };

            return new ResourceCrawledEventArgs
            {
                Resource = resource,
                Summary = summary,
                Issues = issues
                    .Select(it => new SeoCheckIssue(it.Alias, it.Severity, null, uri))
                    .ToArray(),
                Completed = completed,
                Discovered = completed + 5
            };
        }

        [Test]
        public void Handle_BuffersUntilTheBatchIsFull()
        {
            var (writer, repository) = Build(batchSize: 3);

            writer.Handle(null, Crawled("https://example.com/1", 1));
            writer.Handle(null, Crawled("https://example.com/2", 2));

            Assert.That(repository.Batches, Is.Empty);

            writer.Handle(null, Crawled("https://example.com/3", 3));

            Assert.Multiple(() =>
            {
                Assert.That(repository.Batches, Has.Count.EqualTo(1));
                Assert.That(repository.Batches[0], Has.Count.EqualTo(3));
            });
        }

        [Test]
        public void Complete_WritesWhateverIsStillBuffered()
        {
            var (writer, repository) = Build(batchSize: 10);

            writer.Handle(null, Crawled("https://example.com/1", 1));
            writer.Handle(null, Crawled("https://example.com/2", 2));
            writer.Complete(EmptyResult());

            Assert.That(repository.AllWrites.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Complete_DoesNotWriteAnEmptyBatch()
        {
            var (writer, repository) = Build(batchSize: 2);

            writer.Handle(null, Crawled("https://example.com/1", 1));
            writer.Handle(null, Crawled("https://example.com/2", 2));
            writer.Complete(EmptyResult());

            Assert.That(repository.Batches, Has.Count.EqualTo(1), "the buffer was already empty");
        }

        [Test]
        public void Handle_CarriesIndexabilityThroughFromTheCrawlRatherThanRecomputingIt()
        {
            var (writer, repository) = Build(batchSize: 1);

            writer.Handle(null, Crawled("https://example.com/1", 1));

            Assert.That(repository.AllWrites.Single().Resource.Indexability,
                Is.EqualTo(IndexabilityFlags.NoIndex));
        }

        [Test]
        public void ApplyTotals_SummarisesSeveritiesAcrossTheRun()
        {
            var (writer, _) = Build(batchSize: 10);

            writer.Handle(null, Crawled("https://example.com/1", 1,
                (SeoSeverity.Error, "A"), (SeoSeverity.Warning, "B")));
            writer.Handle(null, Crawled("https://example.com/2", 2,
                (SeoSeverity.Critical, "C"), (SeoSeverity.Notice, "D"), (SeoSeverity.Error, "A")));

            var run = new AuditRun { StartingUrl = new Uri("https://example.com/") };
            writer.ApplyTotals(run, Result(crawled: 2, discovered: 7));

            Assert.Multiple(() =>
            {
                Assert.That(run.CriticalCount, Is.EqualTo(1));
                Assert.That(run.ErrorCount, Is.EqualTo(2));
                Assert.That(run.WarningCount, Is.EqualTo(1));
                Assert.That(run.NoticeCount, Is.EqualTo(1));
                Assert.That(run.TotalCrawled, Is.EqualTo(2));
                Assert.That(run.TotalDiscovered, Is.EqualTo(7));
            });
        }

        [Test]
        public void Complete_WritesSiteWideFindingsSeparately()
        {
            var (writer, repository) = Build(batchSize: 10);

            var siteIssue = new SeoCheckIssue("Site.Check", SeoSeverity.Warning);
            var pageIssue = new SeoCheckIssue("Page.Check", SeoSeverity.Error, null, new Uri("https://example.com/1"));

            writer.Complete(Result(crawled: 0, discovered: 0, issues: new[] { siteIssue, pageIssue }));

            Assert.Multiple(() =>
            {
                Assert.That(repository.SiteIssues, Has.Count.EqualTo(1));
                Assert.That(repository.SiteIssues[0].CheckAlias, Is.EqualTo("Site.Check"));
            });
        }

        [Test]
        public void Complete_StoresThePerCheckTotals()
        {
            var (writer, repository) = Build(batchSize: 10);

            var stats = new CheckRunStats
            {
                Alias = "Sample.Check",
                Name = "Sample",
                Category = SeoCheckCategory.OnPage,
                Weight = 7,
                Severity = SeoSeverity.Warning,
                DidRun = true
            };
            stats.ApplicableCount = 12;
            stats.FailedCount = 3;

            writer.Complete(Result(crawled: 12, discovered: 12, checkStats: new[] { stats }));

            var stored = repository.CheckRuns.Single();

            Assert.Multiple(() =>
            {
                Assert.That(stored.CheckAlias, Is.EqualTo("Sample.Check"));
                Assert.That(stored.ApplicableCount, Is.EqualTo(12));
                Assert.That(stored.FailedCount, Is.EqualTo(3));
                Assert.That(stored.Weight, Is.EqualTo(7));
                Assert.That(stored.DidRun, Is.True);
            });
        }

        [Test]
        public void Handle_IsSafeToCallFromSeveralCrawlThreads()
        {
            var (writer, repository) = Build(batchSize: 10);

            Parallel.For(0, 200, i =>
                writer.Handle(null, Crawled($"https://example.com/{i}", i, (SeoSeverity.Error, "A"))));

            writer.Complete(EmptyResult());

            var run = new AuditRun { StartingUrl = new Uri("https://example.com/") };
            writer.ApplyTotals(run, null!);

            Assert.Multiple(() =>
            {
                Assert.That(repository.AllWrites.Count(), Is.EqualTo(200));
                Assert.That(run.ErrorCount, Is.EqualTo(200));
            });
        }

        [Test]
        public void Heartbeat_ReportsProgressWithoutTouchingTheResultsTables()
        {
            var (writer, repository) = Build(batchSize: 100);

            writer.Handle(null, Crawled("https://example.com/1", 1));
            writer.Heartbeat();

            Assert.Multiple(() =>
            {
                Assert.That(repository.HeartbeatCount, Is.EqualTo(1));
                Assert.That(repository.Batches, Is.Empty, "nothing was flushed just to report progress");
            });
        }

        private static CrawlResult EmptyResult() => Result(0, 0);

        private static CrawlResult Result(int crawled, int discovered,
            IReadOnlyList<SeoCheckIssue>? issues = null,
            IReadOnlyList<CheckRunStats>? checkStats = null)
            => new()
            {
                Index = new CrawlIndex(),
                Issues = issues ?? Array.Empty<SeoCheckIssue>(),
                CheckStats = checkStats ?? Array.Empty<CheckRunStats>(),
                Crawled = crawled,
                Discovered = discovered,
                WasCancelled = false
            };
    }

    [TestFixture]
    public class AuditRunProgressTests
    {
        private static AuditRun Run(SiteAuditStatus status, int crawled = 0, int discovered = 0, int? maxPages = null)
            => new()
            {
                StartingUrl = new Uri("https://example.com/"),
                Status = status,
                TotalCrawled = crawled,
                TotalDiscovered = discovered,
                MaxPages = maxPages
            };

        [Test]
        public void Progress_IsMeasuredAgainstTheLimitWhenThereIsOne()
        {
            Assert.That(Run(SiteAuditStatus.Running, crawled: 25, discovered: 900, maxPages: 100).Progress,
                Is.EqualTo(25));
        }

        [Test]
        public void Progress_IsMeasuredAgainstWhatHasBeenDiscoveredOtherwise()
        {
            Assert.That(Run(SiteAuditStatus.Running, crawled: 5, discovered: 20).Progress, Is.EqualTo(25));
        }

        [Test]
        public void Progress_IsMeasuredAgainstTheSiteWhenTheLimitIsLargerThanIt()
        {
            // A limit of 100 on a site with 17 pages is not what the crawl is working towards,
            // and measuring against it leaves the bar stuck short of the end.
            Assert.That(Run(SiteAuditStatus.Running, crawled: 17, discovered: 17, maxPages: 100).Progress,
                Is.EqualTo(100));
        }

        [Test]
        public void Progress_NeverExceedsOneHundred()
        {
            //More pages can be discovered than the limit allows, and the bar must not overflow.
            Assert.That(Run(SiteAuditStatus.Running, crawled: 150, discovered: 150, maxPages: 100).Progress,
                Is.EqualTo(100));
        }

        [TestCase(SiteAuditStatus.Finished)]
        [TestCase(SiteAuditStatus.Error)]
        [TestCase(SiteAuditStatus.Stopped)]
        [TestCase(SiteAuditStatus.Interrupted)]
        public void Progress_IsCompleteForAnyRestingState(SiteAuditStatus status)
        {
            Assert.Multiple(() =>
            {
                Assert.That(Run(status).Progress, Is.EqualTo(100));
                Assert.That(Run(status).IsFinished, Is.True);
            });
        }

        [TestCase(SiteAuditStatus.Created)]
        [TestCase(SiteAuditStatus.Scheduled)]
        public void AQueuedRun_HasNotStartedAndIsNotFinished(SiteAuditStatus status)
        {
            Assert.Multiple(() =>
            {
                Assert.That(Run(status).Progress, Is.Zero);
                Assert.That(Run(status).IsFinished, Is.False);
            });
        }

        [Test]
        public void Progress_IsZeroBeforeAnythingHasBeenDiscovered()
        {
            Assert.That(Run(SiteAuditStatus.Running).Progress, Is.Zero);
        }
    }
}
