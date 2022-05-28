using System.Collections.Generic;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Sitemap.Core.Notifications
{
    public class GenerateSitemapNotification : INotification
    {
        public List<SitemapNodeItem> Nodes { get; }

        public GenerateSitemapNotification(List<SitemapNodeItem> nodes)
        {
            Nodes = nodes;
        }
    }
}
