using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Redirects.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SeoToolkit.Umbraco.Redirects.Core.BackgroundTasks
{
    internal class RebuildBloomFilterTask : RecurringHostedServiceBase
    {
        private readonly IRedirectsBloomFilter _redirectsBloomFilter;
        private readonly IRuntimeState _runtimeState;

        public RebuildBloomFilterTask(IRuntimeState runtimeState, ILogger<RebuildBloomFilterTask> logger, IRedirectsBloomFilter redirectsBloomFilter) : base(logger, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(0))
        {
            _redirectsBloomFilter = redirectsBloomFilter;
            _runtimeState = runtimeState;
        }
        public override Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return Task.CompletedTask;

            if (_redirectsBloomFilter.ShouldRebuild)
            {
                _redirectsBloomFilter.Rebuild();
            }
            return Task.CompletedTask;
        }
    }
}
