using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Notifications
{
    public class RobotsTxtSavedNotification : INotification
    {
        public RobotsTxtModel Model { get; }

        public RobotsTxtSavedNotification(RobotsTxtModel model)
        {
            Model = model;
        }
    }
}
