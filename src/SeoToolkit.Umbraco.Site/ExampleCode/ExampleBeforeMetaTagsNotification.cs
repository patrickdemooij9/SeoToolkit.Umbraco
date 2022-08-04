using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Site.ExampleCode
{
    public class ExampleBeforeMetaTagsNotification : INotificationHandler<BeforeMetaTagsNotification>
    {
        public void Handle(BeforeMetaTagsNotification notification)
        {
            if (notification.ContentTypeAlias.Equals("home"))
            {
                notification.MetaTags.Title = "Hello world!";
                return;
            }
        }
    }
}
