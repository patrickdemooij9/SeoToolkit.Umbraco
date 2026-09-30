using System;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.MetaFields.Core.Notifications
{
    /// <summary>
    /// Published whenever a node's MetaFields user values are added, updated or deleted.
    /// Carries the node key so listeners can react per node (e.g. refreshing the Deploy
    /// disk artifact for that node).
    /// </summary>
    public class MetaFieldsValueChangedNotification : INotification
    {
        public Guid NodeKey { get; }

        public MetaFieldsValueChangedNotification(Guid nodeKey)
        {
            NodeKey = nodeKey;
        }
    }
}
