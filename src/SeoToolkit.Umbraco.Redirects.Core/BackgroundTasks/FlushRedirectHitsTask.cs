using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SeoToolkit.Umbraco.Redirects.Core.BackgroundTasks
{
    internal class FlushRedirectHitsTask : RecurringHostedServiceBase
    {
        private readonly IRedirectHitTracker _redirectHitTracker;
        private readonly IRuntimeState _runtimeState;

        public FlushRedirectHitsTask(IRuntimeState runtimeState, ILogger<FlushRedirectHitsTask> logger, IRedirectHitTracker redirectHitTracker) : base(logger, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
            _redirectHitTracker = redirectHitTracker;
            _runtimeState = runtimeState;
        }

        public override Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return Task.CompletedTask;

            _redirectHitTracker.Flush();
            return Task.CompletedTask;
        }
    }
}
