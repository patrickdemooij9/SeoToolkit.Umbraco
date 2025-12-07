using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
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

            builder.AddNotificationAsyncHandler<SitemapPageSettingsSavedNotification, SitemapSettingsHandler>();

            builder.AddNotificationAsyncHandler<MetaFieldSettingsSavedNotification, MetaFieldsHandler>();

            builder.AddNotificationAsyncHandler<SeoSettingSavedNotification, SeoSettingHandler>();
        }
    }
}
