#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Umbraco.SiteAudit.Core.BackgroundTasks
{
    /// <summary>
    /// Picks up queued audits and runs them.
    /// <para>
    /// This is what makes an audit survive the request that asked for it. Previously the crawl
    /// ran inside the controller on a detached task, so an application recycle lost it silently
    /// and the run stayed at Running forever - and the "will begin within a minute" message the
    /// backoffice shows was never true, because the scheduler it referred to was never wired up.
    /// </para>
    /// <para>
    /// Only one server in a load balanced setup does this work. That is enforced twice over: the
    /// role check here, and an atomic claim on the run itself, so even a misconfigured
    /// environment cannot run the same audit from two places.
    /// </para>
    /// </summary>
    public class SiteAuditJobRunner : RecurringHostedServiceBase
    {
        private static readonly TimeSpan Period = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(30);

        /// <summary>How many runs are kept before the oldest are removed.</summary>
        public const int RetainedRuns = 20;

        private readonly IRuntimeState _runtimeState;
        private readonly IServerRoleAccessor _serverRoleAccessor;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<SiteAuditJobRunner> _logger;

        private bool _isExecuting;

        public SiteAuditJobRunner(IRuntimeState runtimeState,
            IServerRoleAccessor serverRoleAccessor,
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime lifetime,
            ILogger<SiteAuditJobRunner> logger)
            : base(logger, Period, InitialDelay)
        {
            _runtimeState = runtimeState;
            _serverRoleAccessor = serverRoleAccessor;
            _serviceScopeFactory = serviceScopeFactory;
            _lifetime = lifetime;
            _logger = logger;
        }

        public override async Task PerformExecuteAsync(object? state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return;

            // Only the scheduling server runs audits. A crawl is expensive and hits the site from
            // outside, so having every node in a load balanced setup do it would multiply both
            // the load on the site and the results stored.
            if (_serverRoleAccessor.CurrentServerRole is not (ServerRole.SchedulingPublisher or ServerRole.Single))
                return;

            // A crawl easily outlasts the tick interval, so skip rather than pile up.
            if (_isExecuting) return;

            _isExecuting = true;
            try
            {
                // A scope of its own: this runs outside any request, and the repository needs one.
                using var serviceScope = _serviceScopeFactory.CreateScope();
                var service = serviceScope.ServiceProvider.GetRequiredService<SiteAuditRunService>();

                Recover(service);

                var queued = service.GetQueuedRuns(1);
                if (queued.Count == 0) return;

                var run = queued[0];

                // Whoever wins the claim owns the run. Losing it simply means another server got
                // there first, which is not a problem worth logging as one.
                if (!service.TryClaim(run)) return;

                _logger.LogInformation("Starting site audit {RunId} ({Name}).", run.Id, run.Name);

                // Tied to application shutdown, so a restart stops the crawl rather than leaving
                // it to be discovered as stale several minutes later.
                await service.ExecuteAsync(run, _lifetime.ApplicationStopping).ConfigureAwait(false);

                _logger.LogInformation("Finished site audit {RunId} with status {Status}.", run.Id, run.Status);

                service.ApplyRetention(RetainedRuns);
            }
            catch (Exception ex)
            {
                // The tick must never be allowed to fail permanently, or no audit ever runs again.
                _logger.LogError(ex, "The site audit job runner failed.");
            }
            finally
            {
                _isExecuting = false;
            }
        }

        private void Recover(SiteAuditRunService service)
        {
            try
            {
                var recovered = service.RecoverStaleRuns();
                if (recovered > 0)
                    _logger.LogWarning("Marked {Count} site audit(s) as interrupted after they stopped reporting.", recovered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not recover stale site audits.");
            }
        }
    }
}
