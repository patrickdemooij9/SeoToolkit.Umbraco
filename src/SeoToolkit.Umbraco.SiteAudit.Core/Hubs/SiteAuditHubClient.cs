using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Hubs
{
    public class SiteAuditHubClient
    {
        public string ClientId { get; }
        public int AuditId { get; }
        public DateTime LastAccessed { get; set; }

        public SiteAuditHubClient(string clientId, int auditId)
        {
            ClientId = clientId;
            AuditId = auditId;
            LastAccessed = DateTime.UtcNow;
        }
    }
}
