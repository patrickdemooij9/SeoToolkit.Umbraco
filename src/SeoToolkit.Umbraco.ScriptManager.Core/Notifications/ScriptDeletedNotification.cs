using System;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Notifications
{
    public class ScriptDeletedNotification : INotification
    {
        public Guid Key { get; }

        public ScriptDeletedNotification(Guid key)
        {
            Key = key;
        }
    }
}
