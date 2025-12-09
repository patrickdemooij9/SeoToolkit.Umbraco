using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Redirects.Core.Notifications
{
    public class RedirectSavedNotification : INotification
    {
        public Redirect Model { get; }

        public RedirectSavedNotification(Redirect model)
        {
            Model = model;
        }
    }
}
