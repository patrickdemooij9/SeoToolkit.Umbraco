using System;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Common.Core.Notifications
{
    public class SeoKeyValueSavedNotification : INotification
    {
        public Guid? DomainCollectionId { get; }

        public SeoKeyValueSavedNotification(Guid? domainCollectionId)
        {
            DomainCollectionId = domainCollectionId;
        }
    }
}
