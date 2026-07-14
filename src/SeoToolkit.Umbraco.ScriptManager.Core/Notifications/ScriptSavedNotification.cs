using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Notifications
{
    public class ScriptSavedNotification : INotification
    {
        public Script Script { get; }

        public ScriptSavedNotification(Script script)
        {
            Script = script;
        }
    }
}
