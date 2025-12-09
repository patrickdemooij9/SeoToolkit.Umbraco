using System;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Common.Core.Notifications
{
    public class SeoSettingSavedNotification : INotification
    {
        public Guid ContentTypeGuid { get; }
        public bool IsEnabled { get; }

        public SeoSettingSavedNotification(Guid contentTypeGuid, bool isEnabled)
        {
            ContentTypeGuid = contentTypeGuid;
            IsEnabled = isEnabled;
        }
    }
}
