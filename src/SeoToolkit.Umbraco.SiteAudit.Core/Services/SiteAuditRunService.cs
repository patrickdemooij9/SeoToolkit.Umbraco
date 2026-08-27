#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Scoping;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Scoring;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    /// <summary>
    /// Owns the lifecycle of an audit run: queueing one, executing it, and stopping it.
    /// <para>
    /// Execution is deliberately separated from the request that asked for it. The previous
    /// design started the crawl inside the controller with a fire-and-forget
    /// <c>Task.Run(... .Result)</c>, so an application recycle part way through simply lost the
    /// run and left it showing as Running forever. Here a run is queued, and a background service
    /// picks it up and can be trusted to finish it or mark it as interrupted.
    /// </para>
    /// </summary>
    public class SiteAuditRunService
    {
        /// <summary>A run whose heartbeat is older than this is assumed dead.</summary>
        public static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(5);

        /// <summary>How often progress is written back while a crawl is going.</summary>
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(15);

        private readonly ISiteAuditRunRepository _repository;
        private readonly ICrawlEngineFactory _engineFactory;
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<SiteAuditRunService> _logger;

        /// <summary>
        /// Runs executing on this server, so a stop request can cancel one. Static because the
        /// service itself is resolved per use, while a crawl outlives any one of those.
        /// </summary>
        private static readonly ConcurrentDictionary<int, RunControl> Running = new();

        /// <summary>
        /// Lets a cancelled crawl be told apart from an abandoned one. A person stopping an audit
        /// and the application shutting down mid-crawl both cancel the same token, but they mean
        /// very different things to whoever reads the result afterwards.
        /// </summary>
        private sealed class RunControl
        {
            public required CancellationTokenSource Cancellation { get; init; }
            public bool StoppedByUser;
        }

        public SiteAuditRunService(ISiteAuditRunRepository repository,
            ICrawlEngineFactory engineFactory,
            IScopeProvider scopeProvider,
            ILogger<SiteAuditRunService> logger)
        {
            _repository = repository;
            _engineFactory = engineFactory;
            _scopeProvider = scopeProvider;
            _logger = logger;
        }

        // ---- reads --------------------------------------------------------------------------

        public AuditRun? Get(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.Get(id);
        }

        public PagedModel<AuditRun> GetPaged(int skip, int take)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.GetPaged(skip, take);
        }

        public PagedModel<AuditResource> GetResources(int runId, int skip, int take, AuditResourceFilter? filter = null)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.GetResources(runId, skip, take, filter);
        }

        public PagedModel<AuditIssue> GetIssues(int runId, int skip, int take, AuditIssueFilter? filter = null)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.GetIssues(runId, skip, take, filter);
        }

        public IReadOnlyList<AuditCheckRun> GetCheckRuns(int runId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.GetCheckRuns(runId);
        }

        public AuditResource? GetResource(int runId, int resourceId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.GetResource(runId, resourceId);
        }

        public IReadOnlyList<AuditIssue> GetIssuesForResource(int runId, int resourceId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.GetIssuesForResource(runId, resourceId);
        }

        public IReadOnlyList<AuditRun> GetQueuedRuns(int max)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            return _repository.GetQueued(max);
        }

        /// <summary>
        /// Claims a queued run for this server. Returns false when another server claimed it
        /// first, which is what keeps a load balanced setup from crawling the same site twice.
        /// </summary>
        public bool TryClaim(AuditRun run)
        {
            if (run is null) throw new ArgumentNullException(nameof(run));

            using var scope = _scopeProvider.CreateScope();
            var claimed = _repository.TryClaim(run.Id, Environment.MachineName);
            scope.Complete();

            if (!claimed) return false;

            // Bring the in-memory copy in line with what the claim wrote. Without this the run is
            // saved again at the end of the crawl with its original values, and the started time
            // and owner recorded by the claim are lost.
            run.Status = SiteAuditStatus.Running;
            run.ClaimedBy = Environment.MachineName;
            run.StartedUtc = DateTime.UtcNow;
            run.HeartbeatUtc = run.StartedUtc;

            return true;
        }

        // ---- lifecycle ----------------------------------------------------------------------

        /// <summary>
        /// Stores a run and marks it ready to be picked up. Returns immediately - nothing is
        /// crawled on the caller's thread.
        /// </summary>
        public AuditRun Queue(AuditRun run)
        {
            if (run is null) throw new ArgumentNullException(nameof(run));

            run.Status = SiteAuditStatus.Scheduled;
            run.CreatedUtc = run.CreatedUtc == default ? DateTime.UtcNow : run.CreatedUtc;

            using var scope = _scopeProvider.CreateScope();
            var saved = _repository.Save(run);
            scope.Complete();

            return saved;
        }

        public void Delete(int id)
        {
            RequestStop(id);

            using var scope = _scopeProvider.CreateScope();
            _repository.Delete(id);
            scope.Complete();
        }

        /// <summary>
        /// Asks a running crawl to stop. Returns false when it is not running on this server -
        /// which behind a load balancer is entirely normal, and is why the status is also
        /// checked by whichever server owns it.
        /// </summary>
        public bool RequestStop(int runId)
        {
            if (!Running.TryGetValue(runId, out var control)) return false;

            control.StoppedByUser = true;
            control.Cancellation.Cancel();
            return true;
        }

        public bool IsRunningHere(int runId) => Running.ContainsKey(runId);

        /// <summary>Removes all but the newest runs, so history does not grow without bound.</summary>
        public int ApplyRetention(int keep)
        {
            using var scope = _scopeProvider.CreateScope();
            var removed = _repository.DeleteAllButNewest(keep);
            scope.Complete();

            return removed;
        }

        /// <summary>
        /// Marks runs that stopped reporting as interrupted. Without this a crawl killed by an
        /// application restart sits at Running forever and the UI waits on it indefinitely.
        /// </summary>
        public int RecoverStaleRuns()
        {
            using var scope = _scopeProvider.CreateScope();
            var recovered = _repository.MarkStaleRunsInterrupted(StaleAfter);
            scope.Complete();

            return recovered;
        }

        /// <summary>
        /// Checks a single page without storing anything.
        /// <para>
        /// Used by the content app, where an editor wants an immediate answer about the page they
        /// are looking at. Only checks that make sense on one page in isolation take part - a
        /// broken-link check, for instance, is about the whole site rather than this page.
        /// </para>
        /// </summary>
        public async Task<IReadOnlyList<AuditIssue>> RunSinglePageAsync(Uri url, CancellationToken cancellationToken = default)
        {
            if (url is null) throw new ArgumentNullException(nameof(url));

            var options = _engineFactory.CreateOptions(url, maxPages: 1, delayMs: 0);
            var checks = _engineFactory.GetChecks()
                .Where(it => it.Descriptor.SupportsSinglePage)
                .ToArray();

            var engine = _engineFactory.Create(options);

            var run = new AuditRun { StartingUrl = url, MaxPages = 1 };
            var context = new AuditRunContext(run, _engineFactory.GetAvailableCapabilities());

            var result = await engine
                .CrawlAsync(url, options, checks, context, RobotsTxtMatcher.AllowAll, null, cancellationToken)
                .ConfigureAwait(false);

            return result.Issues.Select(AuditResultWriter.MapIssue).ToArray();
        }

        /// <summary>
        /// Runs a crawl to completion. The run must already have been claimed by this server.
        /// </summary>
        public async Task<AuditRun> ExecuteAsync(AuditRun run, CancellationToken cancellationToken = default)
        {
            if (run is null) throw new ArgumentNullException(nameof(run));

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var control = new RunControl { Cancellation = cancellation };

            if (!Running.TryAdd(run.Id, control))
            {
                _logger.LogWarning("Site audit {RunId} is already running on this server.", run.Id);
                return run;
            }

            var writer = new AuditResultWriter(run.Id, _repository, _scopeProvider);

            try
            {
                var options = _engineFactory.CreateOptions(run.StartingUrl, run.MaxPages, run.DelayMs, run.BaseUrl);
                var entryPoints = await _engineFactory.CreateEntryPointProvider()
                    .DiscoverAsync(run.StartingUrl, options, cancellation.Token)
                    .ConfigureAwait(false);

                var engine = _engineFactory.Create(options, entryPoints.Robots);
                engine.ResourceCrawled += writer.Handle;

                using var heartbeat = StartHeartbeat(writer, cancellation.Token);

                var result = await engine.CrawlAsync(
                    run.StartingUrl,
                    options,
                    SelectChecks(run),
                    new AuditRunContext(run, _engineFactory.GetAvailableCapabilities()),
                    entryPoints.Robots,
                    entryPoints.SitemapUrls,
                    cancellation.Token).ConfigureAwait(false);

                engine.ResourceCrawled -= writer.Handle;

                writer.Complete(result);
                writer.ApplyTotals(run, result);

                // Scored from the per-check totals the crawl just recorded, so this never reads
                // the results tables back.
                var score = new HealthScoreCalculator().Calculate(
                    result.CheckStats.Select(AuditResultWriter.MapCheckRun).ToArray());

                run.Score = score.HasScore ? score.Score : null;
                run.ScoreVersion = score.Version;

                run.Status = result.WasCancelled
                    // Someone pressing stop keeps what was gathered and says so. Anything else
                    // cancelling the crawl - an application shutting down, most likely - is not
                    // a decision anyone made, so it is reported as interrupted instead.
                    ? (control.StoppedByUser ? SiteAuditStatus.Stopped : SiteAuditStatus.Interrupted)
                    : SiteAuditStatus.Finished;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Site audit {RunId} failed.", run.Id);
                run.Status = SiteAuditStatus.Error;
            }
            finally
            {
                Running.TryRemove(run.Id, out _);

                run.FinishedUtc = DateTime.UtcNow;
                run.HeartbeatUtc = DateTime.UtcNow;

                try
                {
                    using var scope = _scopeProvider.CreateScope();
                    _repository.Save(run);
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    // Losing the final status would leave the run looking as though it is still
                    // going, so this is worth its own log entry.
                    _logger.LogError(ex, "Could not store the final state of site audit {RunId}.", run.Id);
                }
            }

            return run;
        }

        /// <summary>
        /// Writes progress back periodically. This is what the backoffice reads to show progress,
        /// and what tells a later tick that a run died rather than simply being slow.
        /// </summary>
        private IDisposable StartHeartbeat(AuditResultWriter writer, CancellationToken cancellationToken)
        {
            var timer = new Timer(_ =>
            {
                try
                {
                    writer.Heartbeat();
                }
                catch (Exception ex)
                {
                    // A missed heartbeat is recoverable; failing the crawl over one is not sensible.
                    _logger.LogDebug(ex, "Could not write a site audit heartbeat.");
                }
            }, null, HeartbeatInterval, HeartbeatInterval);

            cancellationToken.Register(() => timer.Change(Timeout.Infinite, Timeout.Infinite));

            return timer;
        }

        /// <summary>
        /// The checks this run asked for. A run with none recorded uses every enabled check,
        /// which is what a run created before the selection was stored should fall back to.
        /// </summary>
        private IReadOnlyList<Checks.Abstractions.ISeoCheck> SelectChecks(AuditRun run)
        {
            var available = _engineFactory.GetChecks();
            var selected = AuditRunConfig.FromJson(run.ConfigJson).Checks;

            if (selected.Length == 0) return available;

            var wanted = new HashSet<string>(selected, StringComparer.OrdinalIgnoreCase);
            var result = available.Where(it => wanted.Contains(it.Descriptor.Alias)).ToArray();

            // A run whose checks have all since been removed would otherwise crawl the whole site
            // and report nothing, which looks like a broken audit rather than a configuration problem.
            if (result.Length == 0)
            {
                _logger.LogWarning(
                    "Site audit {RunId} selected {Count} check(s), none of which are installed. Running all enabled checks instead.",
                    run.Id, selected.Length);
                return available;
            }

            return result;
        }

        /// <summary>Exposes the run to checks as the crawl context.</summary>
        private sealed class AuditRunContext : Checks.Abstractions.ISeoAuditRunContext
        {
            private readonly AuditRun _run;

            public AuditRunContext(AuditRun run, Checks.Abstractions.SeoCheckCapabilities capabilities)
            {
                _run = run;
                AvailableCapabilities = capabilities;
            }

            public int RunId => _run.Id;
            public Uri StartingUrl => _run.StartingUrl;
            public string? BaseUrl => _run.BaseUrl;
            public bool IsSinglePage => _run.MaxPages == 1;
            public string UserAgent => _run.UserAgent ?? "SeoToolkit-SiteAudit";
            public Checks.Abstractions.SeoCheckCapabilities AvailableCapabilities { get; }

            public bool IsFeatureEnabled(string feature)
                => Common.Core.Helpers.SeoFeatureRegistry.IsEnabled(feature);
        }
    }
}
