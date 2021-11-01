using Umbraco.Cms.Core.Notifications;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Notifications
{
    public class SiteAuditUpdatedNotification : INotification
    {
        public SiteAuditDto SiteAudit { get; }

        public SiteAuditUpdatedNotification(SiteAuditDto siteAudit)
        {
            SiteAudit = siteAudit;
        }
    }
}
