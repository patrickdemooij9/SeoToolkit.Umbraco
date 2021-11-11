using System.Collections.Concurrent;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Common.Scheduler
{
    public class SiteAuditScheduler : ISiteAuditScheduler
    {
        private readonly ConcurrentQueue<int> _scheduledSiteAudits;

        public SiteAuditScheduler()
        {
            _scheduledSiteAudits = new ConcurrentQueue<int>();
        }

        public int? GetNextSiteAuditsToRun()
        {
            if (_scheduledSiteAudits.IsEmpty) return null;
            if (_scheduledSiteAudits.TryDequeue(out var result))
                return result;
            return null;
        }

        public void AddSiteAudit(int id)
        {
            _scheduledSiteAudits.Enqueue(id);
        }
    }
}
