using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.SiteAudit.Core.Hubs;
using System;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace SeoToolkit.Umbraco.SiteAudit.Core.BackgroundTasks
{
    public class SiteAuditHubClientCleanup : RecurringHostedServiceBase
    {
        private readonly SiteAuditHubClientService _siteAuditHubClientService;

        public SiteAuditHubClientCleanup(ILogger<SiteAuditHubClient> logger, SiteAuditHubClientService siteAuditHubClientService) : base(logger, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30))
        {
            _siteAuditHubClientService = siteAuditHubClientService;
        }

        public override Task PerformExecuteAsync(object state)
        {
            _siteAuditHubClientService.Cleanup();
            return Task.CompletedTask;
        }
    }
}
