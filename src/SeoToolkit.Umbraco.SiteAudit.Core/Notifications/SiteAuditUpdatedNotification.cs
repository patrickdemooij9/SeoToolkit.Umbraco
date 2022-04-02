using Umbraco.Cms.Core.Notifications;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Notifications
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
