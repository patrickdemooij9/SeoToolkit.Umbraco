using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Site.ExampleCode
{
    public class ExampleSitemapNodeNotification : INotificationHandler<GenerateSitemapNodeNotification>
    {
        public void Handle(GenerateSitemapNodeNotification notification)
        {
            notification.Node.Priority ??= 0.5;
            notification.Node.HideFromSitemap |= notification.Node.Content.Value<bool>("hideFromSitemap");
        }
    }
}
