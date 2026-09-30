using System;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Sitemap.Core.Notifications
{
    /// <summary>
    /// Published whenever a node's sitemap content settings are set or reset-to-default (deleted).
    /// Carries the node key so listeners can react per node (e.g. refreshing the Deploy disk
    /// artifact for that node).
    /// </summary>
    public class SitemapContentChangedNotification : INotification
    {
        public Guid NodeKey { get; }

        public SitemapContentChangedNotification(Guid nodeKey)
        {
            NodeKey = nodeKey;
        }
    }
}
