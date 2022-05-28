using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Sitemap.Core.Notifications
{
    public class GenerateSitemapNodeNotification : INotification
    {
        public SitemapNodeItem Node { get; }

        public GenerateSitemapNodeNotification(SitemapNodeItem node)
        {
            Node = node;
        }
    }
}
