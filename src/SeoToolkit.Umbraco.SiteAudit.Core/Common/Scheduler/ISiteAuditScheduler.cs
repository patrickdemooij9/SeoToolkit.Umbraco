using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Common.Scheduler
{
    public interface ISiteAuditScheduler
    {
        int? GetNextSiteAuditsToRun();
        void AddSiteAudit(int id);
    }
}
