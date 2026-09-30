using SeoToolkit.Umbraco.Common.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Common.Core.Notifications
{
    public class SeoDomainCollectionSavedNotification : INotification
    {
        public SeoDomainCollection Collection { get; }

        public SeoDomainCollectionSavedNotification(SeoDomainCollection collection)
        {
            Collection = collection;
        }
    }
}
