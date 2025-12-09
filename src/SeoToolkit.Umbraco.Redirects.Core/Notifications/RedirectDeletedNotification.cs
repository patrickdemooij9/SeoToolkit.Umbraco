using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Redirects.Core.Notifications
{
    public class RedirectDeletedNotification : INotification
    {
        public Redirect Model { get; }

        public RedirectDeletedNotification(Redirect model)
        {
            Model = model;
        }
    }
}
