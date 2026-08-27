using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SeoToolkit.Umbraco.SiteAudit.Core.BackgroundTasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SiteAuditJobRunnerTests
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

        private static (SiteAuditJobRunner Runner, RecordingRunRepository Repository) Build(
            RuntimeLevel level = RuntimeLevel.Run,
            ServerRole role = ServerRole.Single,
            IReadOnlyList<AuditRun>? queued = null)
        {
            var repository = new RecordingRunRepository { Queued = queued ?? Array.Empty<AuditRun>() };

            var site = new FakeSiteHandler()
                .Page(Root, "<html><head><title>A perfectly reasonable title</title></head><body></body></html>");

            var services = new ServiceCollection();
            services.AddSingleton<ISiteAuditRunRepository>(repository);
            services.AddSingleton<ICrawlEngineFactory>(new FakeCrawlEngineFactory(site));
            services.AddSingleton(ScopeProvider());
            services.AddSingleton<Microsoft.Extensions.Logging.ILogger<SiteAuditRunService>>(
                NullLogger<SiteAuditRunService>.Instance);
            services.AddTransient<SiteAuditRunService>();

            var runtimeState = new Mock<IRuntimeState>();
            runtimeState.SetupGet(x => x.Level).Returns(level);

            var serverRole = new Mock<IServerRoleAccessor>();
            serverRole.Setup(x => x.CurrentServerRole).Returns(role);

            var lifetime = new Mock<IHostApplicationLifetime>();
            lifetime.SetupGet(x => x.ApplicationStopping).Returns(CancellationToken.None);

            var runner = new SiteAuditJobRunner(
                runtimeState.Object,
                serverRole.Object,
                services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
                lifetime.Object,
                NullLogger<SiteAuditJobRunner>.Instance);

            return (runner, repository);
        }

        private static AuditRun Queued(int id = 1) => new()
        {
            Id = id,
            Name = "Queued audit",
            StartingUrl = new Uri(Root),
            Status = SiteAuditStatus.Scheduled,
            CreatedUtc = DateTime.UtcNow
        };

        [Test]
        public async Task DoesNothingWhileUmbracoIsNotFullyRunning()
        {
            //Crawling during an install or upgrade would hit a site that is not ready.
            var (runner, repository) = Build(level: RuntimeLevel.Install, queued: new[] { Queued() });

            await runner.PerformExecuteAsync(null);

            Assert.That(repository.SavedRuns, Is.Empty);
        }

        [TestCase(ServerRole.Subscriber)]
        [TestCase(ServerRole.Unknown)]
        public async Task DoesNothingOnAServerThatIsNotTheSchedulingOne(ServerRole role)
        {
            //Every node in a load balanced setup running the crawl would multiply both the load
            //on the site and the results stored.
            var (runner, repository) = Build(role: role, queued: new[] { Queued() });

            await runner.PerformExecuteAsync(null);

            Assert.That(repository.SavedRuns, Is.Empty);
        }

        [TestCase(ServerRole.Single)]
        [TestCase(ServerRole.SchedulingPublisher)]
        public async Task RunsAQueuedAuditOnTheSchedulingServer(ServerRole role)
        {
            var (runner, repository) = Build(role: role, queued: new[] { Queued() });

            await runner.PerformExecuteAsync(null);

            Assert.Multiple(() =>
            {
                Assert.That(repository.SavedRuns, Has.Count.EqualTo(1));
                Assert.That(repository.SavedRuns[0].Status, Is.EqualTo(SiteAuditStatus.Finished));
            });
        }

        [Test]
        public async Task DoesNothingWhenNothingIsQueued()
        {
            var (runner, repository) = Build();

            await runner.PerformExecuteAsync(null);

            Assert.Multiple(() =>
            {
                Assert.That(repository.SavedRuns, Is.Empty);
                Assert.That(repository.StaleAfter, Is.Not.Null, "but stale runs are still recovered");
            });
        }

        [Test]
        public async Task LeavesTheRunAloneWhenAnotherServerClaimsItFirst()
        {
            var (runner, repository) = Build(queued: new[] { Queued() });
            repository.ClaimSucceeds = false;

            await runner.PerformExecuteAsync(null);

            Assert.That(repository.SavedRuns, Is.Empty, "the other server owns it now");
        }

        [Test]
        public async Task RecoversRunsThatStoppedReporting()
        {
            //A crawl killed by an application restart would otherwise sit at Running forever.
            var (runner, repository) = Build();

            await runner.PerformExecuteAsync(null);

            Assert.That(repository.StaleAfter, Is.EqualTo(SiteAuditRunService.StaleAfter));
        }

        [Test]
        public async Task AppliesRetentionAfterARun()
        {
            var (runner, repository) = Build(queued: new[] { Queued() });

            await runner.PerformExecuteAsync(null);

            Assert.That(repository.RetentionKeep, Is.EqualTo(SiteAuditJobRunner.RetainedRuns));
        }

        [Test]
        public async Task TakesOnlyOneRunPerTick()
        {
            //A crawl easily outlasts the tick interval, so they must not pile up.
            var (runner, repository) = Build(queued: new[] { Queued(1), Queued(2), Queued(3) });

            await runner.PerformExecuteAsync(null);

            Assert.That(repository.SavedRuns, Has.Count.EqualTo(1));
        }
    }
}
