using SeoToolkit.Umbraco.Redirects.Core.Notifications;
using SeoToolkit.Umbraco.RobotsTxt.Core.Notifications;
using SeoToolkit.Umbraco.uSync.Handlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.uSync
{
    public class Composer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            //builder.AddNotificationAsyncHandler<RobotsTxtSavedNotification, RobotsTxtHandler>();

            //builder.AddNotificationAsyncHandler<RedirectSavedNotification, RedirectHandler>();
            //builder.AddNotificationAsyncHandler<RedirectDeletedNotification, RedirectHandler>();
        }
    }
}
