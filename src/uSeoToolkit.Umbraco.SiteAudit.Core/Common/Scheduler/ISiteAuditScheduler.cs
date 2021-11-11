using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Common.Scheduler
{
    public interface ISiteAuditScheduler
    {
        int? GetNextSiteAuditsToRun();
        void AddSiteAudit(int id);
    }
}
