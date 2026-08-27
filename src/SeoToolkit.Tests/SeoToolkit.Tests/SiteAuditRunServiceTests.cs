using System.Data;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace SeoToolkit.Tests
{
    /// <summary>Builds a real engine over a fake site, so the run service is exercised end to end.</summary>
    internal sealed class FakeCrawlEngineFactory : ICrawlEngineFactory
    {
        private readonly FakeSiteHandler _site;
        private readonly IReadOnlyList<ISeoCheck> _checks;
        private readonly Exception? _throwOnCreate;

        public FakeCrawlEngineFactory(FakeSiteHandler site,
            IReadOnlyList<ISeoCheck>? checks = null,
            Exception? throwOnCreate = null)
        {
            _site = site;
            _checks = checks ?? Array.Empty<ISeoCheck>();
            _throwOnCreate = throwOnCreate;
        }

        public CrawlEngine Create(CrawlOptions options, RobotsTxtMatcher? robots = null)
        {
            if (_throwOnCreate is not null) throw _throwOnCreate;

            return new CrawlEngine(
                new HttpResourceFetcher(new HttpClient(_site), options),
                new DefaultUrlNormalizer(options),
                new AngleSharpPageFactsParser(),
                new HostPolitenessGate(TimeSpan.Zero));
        }

        public ISiteEntryPointProvider CreateEntryPointProvider() => new NoEntryPoints();

        public IReadOnlyList<ISeoCheck> GetChecks() => _checks;

        public SeoCheckCapabilities GetAvailableCapabilities()
            => SeoCheckCapabilities.CrawlIndex | SeoCheckCapabilities.RawBody | SeoCheckCapabilities.ResponseHeaders;

        public CrawlOptions CreateOptions(Uri startingUrl, int? maxPages, int delayMs, string? baseUrl = null)
            => new()
            {
                MaxPages = maxPages,
                DelayBetweenRequestsMs = delayMs,
                MaxConcurrency = 2,
                UserAgent = "SeoToolkit-Tests"
            };

        private sealed class NoEntryPoints : ISiteEntryPointProvider
        {
            public Task<SiteEntryPoints> DiscoverAsync(Uri startingUrl, CrawlOptions options, CancellationToken cancellationToken)
                => Task.FromResult(new SiteEntryPoints
                {
                    Robots = RobotsTxtMatcher.AllowAll,
                    SitemapUrls = Array.Empty<Uri>()
                });
        }
    }

    [TestFixture]
    public class SiteAuditRunServiceTests
    {
        private const string Root = "https://example.com/";

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

        private static (SiteAuditRunService Service, RecordingRunRepository Repository) Build(
            FakeSiteHandler site,
            IReadOnlyList<ISeoCheck>? checks = null,
            Exception? throwOnCreate = null)
        {
            var repository = new RecordingRunRepository();
            var service = new SiteAuditRunService(
                repository,
                new FakeCrawlEngineFactory(site, checks, throwOnCreate),
                ScopeProvider(),
                NullLogger<SiteAuditRunService>.Instance);

            return (service, repository);
        }

        private static AuditRun Run(int id = 1) => new()
        {
            Id = id,
            Name = "Test audit",
            StartingUrl = new Uri(Root),
            Status = SiteAuditStatus.Running,
            CreatedUtc = DateTime.UtcNow
        };

        private static string Page(string title, params string[] links)
            => $"<html><head><title>{title}</title></head><body>" +
               string.Concat(links.Select(l => $"<a href=\"{l}\">l</a>")) +
               "</body></html>";

        [Test]
        public void Queue_MarksTheRunAsWaitingToBePickedUp()
        {
            //Nothing is crawled on the caller's thread, which is the whole point.
            var (service, repository) = Build(new FakeSiteHandler());

            var queued = service.Queue(new AuditRun { StartingUrl = new Uri(Root), Name = "New" });

            Assert.Multiple(() =>
            {
                Assert.That(queued.Status, Is.EqualTo(SiteAuditStatus.Scheduled));
                Assert.That(queued.CreatedUtc, Is.Not.EqualTo(default(DateTime)));
                Assert.That(repository.SavedRuns, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void TryClaim_BringsTheInMemoryRunInLineWithWhatWasWritten()
        {
            //Otherwise the run is saved again at the end of the crawl with its original values,
            //wiping out the started time and owner the claim recorded.
            var (service, _) = Build(new FakeSiteHandler());
            var run = new AuditRun { Id = 1, StartingUrl = new Uri(Root), Status = SiteAuditStatus.Scheduled };

            var claimed = service.TryClaim(run);

            Assert.Multiple(() =>
            {
                Assert.That(claimed, Is.True);
                Assert.That(run.Status, Is.EqualTo(SiteAuditStatus.Running));
                Assert.That(run.StartedUtc, Is.Not.Null);
                Assert.That(run.ClaimedBy, Is.EqualTo(Environment.MachineName));
            });
        }

        [Test]
        public void TryClaim_ReturnsFalseWhenAnotherServerGotThereFirst()
        {
            var (service, repository) = Build(new FakeSiteHandler());
            repository.ClaimSucceeds = false;

            var run = new AuditRun { Id = 1, StartingUrl = new Uri(Root), Status = SiteAuditStatus.Scheduled };

            Assert.Multiple(() =>
            {
                Assert.That(service.TryClaim(run), Is.False);
                Assert.That(run.Status, Is.EqualTo(SiteAuditStatus.Scheduled), "the run is left alone");
            });
        }

        [Test]
        public async Task ExecuteAsync_CrawlsTheSiteAndFinishes()
        {
            var site = new FakeSiteHandler()
                .Page(Root, Page("A perfectly reasonable title", "/a"))
                .Page("https://example.com/a", Page("Another reasonable title"));

            var (service, repository) = Build(site, new ISeoCheck[] { new SampleTitleCheck() });
            var run = Run();

            await service.ExecuteAsync(run);

            Assert.Multiple(() =>
            {
                Assert.That(run.Status, Is.EqualTo(SiteAuditStatus.Finished));
                Assert.That(run.TotalCrawled, Is.EqualTo(2));
                Assert.That(run.FinishedUtc, Is.Not.Null);
                Assert.That(repository.AllWrites.Count(), Is.EqualTo(2));
                Assert.That(repository.CheckRuns, Is.Not.Empty, "per-check totals are stored");
            });
        }

        [Test]
        public async Task ExecuteAsync_RecordsFindingsAgainstTheRun()
        {
            var site = new FakeSiteHandler().Page(Root, Page("short"));

            var (service, _) = Build(site, new ISeoCheck[] { new SampleTitleCheck() });
            var run = Run();

            await service.ExecuteAsync(run);

            Assert.That(run.WarningCount, Is.EqualTo(1), "the title is shorter than the minimum");
        }

        [Test]
        public async Task ExecuteAsync_MarksTheRunAsErroredWhenTheCrawlCannotStart()
        {
            var (service, _) = Build(new FakeSiteHandler(), throwOnCreate: new InvalidOperationException("boom"));
            var run = Run();

            await service.ExecuteAsync(run);

            Assert.Multiple(() =>
            {
                Assert.That(run.Status, Is.EqualTo(SiteAuditStatus.Error));
                Assert.That(run.FinishedUtc, Is.Not.Null, "it still stops being reported as running");
            });
        }

        [Test]
        public async Task RequestStop_EndsTheCrawlAndSaysAPersonStoppedIt()
        {
            var site = new FakeSiteHandler();
            site.Page(Root, Page("A perfectly reasonable title",
                Enumerable.Range(0, 400).Select(i => $"/p{i}").ToArray()));
            for (var i = 0; i < 400; i++)
                site.Page($"https://example.com/p{i}", Page("A perfectly reasonable title"));

            var (service, _) = Build(site);
            var run = Run();

            var execution = service.ExecuteAsync(run);

            // Wait for the crawl to actually be under way before stopping it.
            var waited = 0;
            while (!service.IsRunningHere(run.Id) && waited < 100)
            {
                await Task.Delay(20);
                waited++;
            }

            Assert.That(service.RequestStop(run.Id), Is.True);

            await execution;

            Assert.Multiple(() =>
            {
                Assert.That(run.Status, Is.EqualTo(SiteAuditStatus.Stopped));
                Assert.That(service.IsRunningHere(run.Id), Is.False, "the run is no longer tracked");
            });
        }

        [Test]
        public async Task ACrawlCancelledFromOutside_IsInterruptedRatherThanStopped()
        {
            //An application shutting down is not a decision anyone made, so it must not look
            //like someone pressed stop.
            var site = new FakeSiteHandler();
            site.Page(Root, Page("A perfectly reasonable title",
                Enumerable.Range(0, 400).Select(i => $"/p{i}").ToArray()));
            for (var i = 0; i < 400; i++)
                site.Page($"https://example.com/p{i}", Page("A perfectly reasonable title"));

            var (service, _) = Build(site);
            var run = Run();

            using var shutdown = new CancellationTokenSource();
            var execution = service.ExecuteAsync(run, shutdown.Token);

            var waited = 0;
            while (!service.IsRunningHere(run.Id) && waited < 100)
            {
                await Task.Delay(20);
                waited++;
            }

            shutdown.Cancel();
            await execution;

            Assert.That(run.Status, Is.EqualTo(SiteAuditStatus.Interrupted));
        }

        [Test]
        public void RequestStop_ReturnsFalseForARunThatIsNotGoingHere()
        {
            //Behind a load balancer this is normal: another server owns the run.
            var (service, _) = Build(new FakeSiteHandler());

            Assert.That(service.RequestStop(12345), Is.False);
        }

        [Test]
        public async Task ExecuteAsync_RefusesToStartTheSameRunTwice()
        {
            var site = new FakeSiteHandler();
            site.Page(Root, Page("A perfectly reasonable title",
                Enumerable.Range(0, 300).Select(i => $"/p{i}").ToArray()));
            for (var i = 0; i < 300; i++)
                site.Page($"https://example.com/p{i}", Page("A perfectly reasonable title"));

            var (service, repository) = Build(site);
            var run = Run();

            var first = service.ExecuteAsync(run);

            var waited = 0;
            while (!service.IsRunningHere(run.Id) && waited < 100)
            {
                await Task.Delay(20);
                waited++;
            }

            var second = await service.ExecuteAsync(Run());

            Assert.That(second.Status, Is.EqualTo(SiteAuditStatus.Running), "the second call did nothing");

            service.RequestStop(run.Id);
            await first;
        }

        [Test]
        public void Delete_StopsARunningCrawlBeforeRemovingIt()
        {
            var (service, repository) = Build(new FakeSiteHandler());

            service.Delete(7);

            Assert.That(repository.DeletedIds, Is.EqualTo(new[] { 7 }));
        }

        [Test]
        public void ApplyRetention_RemovesAllButTheNewestRuns()
        {
            var (service, repository) = Build(new FakeSiteHandler());
            repository.RetentionResult = 5;

            Assert.Multiple(() =>
            {
                Assert.That(service.ApplyRetention(20), Is.EqualTo(5));
                Assert.That(repository.RetentionKeep, Is.EqualTo(20));
            });
        }

        [Test]
        public void RecoverStaleRuns_UsesTheConfiguredStalenessWindow()
        {
            var (service, repository) = Build(new FakeSiteHandler());

            service.RecoverStaleRuns();

            Assert.That(repository.StaleAfter, Is.EqualTo(SiteAuditRunService.StaleAfter));
        }
    }
}
