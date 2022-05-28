using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Site.ExampleCode
{
    public class ExampleSitemapNotification : INotificationHandler<GenerateSitemapNotification>
    {
        public void Handle(GenerateSitemapNotification notification)
        {
            notification.Nodes.Add(new SitemapNodeItem("https://google.nl"));
        }
    }
}
