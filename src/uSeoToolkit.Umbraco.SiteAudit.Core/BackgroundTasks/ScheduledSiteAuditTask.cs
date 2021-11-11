using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;
using uSeoToolkit.Umbraco.SiteAudit.Core.Common.Scheduler;
using uSeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.BackgroundTasks
{
    public class ScheduledSiteAuditTask : RecurringHostedServiceBase
    {
        private readonly IRuntimeState _runtimeState;
        private readonly ISiteAuditScheduler _siteAuditScheduler;
        private readonly SiteAuditService _siteAuditService;

        public ScheduledSiteAuditTask(IRuntimeState runtimeState, ISiteAuditScheduler siteAuditScheduler, SiteAuditService siteAuditService) : base(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
        {
            _runtimeState = runtimeState;
            _siteAuditScheduler = siteAuditScheduler;
            _siteAuditService = siteAuditService;
        }

        public override async Task PerformExecuteAsync(object state)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return;

            var scheduledSiteAuditId = _siteAuditScheduler.GetNextSiteAuditsToRun();
            if (scheduledSiteAuditId is null)
                return;

            await _siteAuditService.StartSiteAudit(_siteAuditService.Get(scheduledSiteAuditId.Value));
        }
    }
}
