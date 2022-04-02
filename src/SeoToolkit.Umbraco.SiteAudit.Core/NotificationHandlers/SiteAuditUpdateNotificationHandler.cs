using System;
using Umbraco.Cms.Core.Events;
using SeoToolkit.Umbraco.SiteAudit.Core.Hubs;
using SeoToolkit.Umbraco.SiteAudit.Core.Notifications;

namespace SeoToolkit.Umbraco.SiteAudit.Core.NotificationHandlers
{
    public class SiteAuditUpdateNotificationHandler : INotificationHandler<SiteAuditUpdatedNotification>
    {
        private readonly SiteAuditHubClientService _hubClient;

        public SiteAuditUpdateNotificationHandler(SiteAuditHubClientService hubClient)
        {
            _hubClient = hubClient;
        }

        public void Handle(SiteAuditUpdatedNotification notification)
        {
            _hubClient.UpdateSiteAudit(notification.SiteAudit);
        }
    }
}
