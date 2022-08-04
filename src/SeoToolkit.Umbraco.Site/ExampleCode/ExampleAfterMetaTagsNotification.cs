using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Site.ExampleCode
{
    public class ExampleAfterMetaTagsNotification : INotificationHandler<AfterMetaTagsNotification>
    {
        public void Handle(AfterMetaTagsNotification notification)
        {
            notification.MetaTags.Title += "Bye!";
        }
    }
}
