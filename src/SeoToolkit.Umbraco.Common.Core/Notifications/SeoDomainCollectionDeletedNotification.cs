using System;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Common.Core.Notifications
{
    public class SeoDomainCollectionDeletedNotification : INotification
    {
        public Guid Id { get; }

        public SeoDomainCollectionDeletedNotification(Guid id)
        {
            Id = id;
        }
    }
}
