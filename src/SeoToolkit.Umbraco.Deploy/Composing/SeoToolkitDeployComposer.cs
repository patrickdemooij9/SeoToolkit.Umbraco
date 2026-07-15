using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Deploy.Core.Events;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<SeoToolkitDeploySettings>()
                .Bind(builder.Config.GetSection("SeoToolkit:Deploy"));

            builder.AddNotificationAsyncHandler<ArtifactExportingNotification, SeoToolkitContentExportingHandler>();

            // Disk (.uda) refreshers: rewrite the settings artifact whenever it is saved/deleted.
            builder.AddNotificationAsyncHandler<SeoSettingSavedNotification, SeoSettingDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<MetaFieldSettingsSavedNotification, MetaFieldsSettingDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SitemapPageSettingsSavedNotification, SitemapPageTypeDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<ScriptSavedNotification, ScriptDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<ScriptDeletedNotification, ScriptDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoDomainCollectionSavedNotification, DomainCollectionDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoDomainCollectionDeletedNotification, DomainCollectionDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoKeyValueSavedNotification, KeyValuesDiskRefresherHandler>();

            // Per-node MetaFields values: refresh (or delete) the node's .uda when its values change.
            builder.AddNotificationAsyncHandler<MetaFieldsValueChangedNotification, MetaFieldsValueDiskRefresherHandler>();

            // Per-node sitemap content: refresh (or delete) the node's .uda when its settings change.
            builder.AddNotificationAsyncHandler<SitemapContentChangedNotification, SitemapContentDiskRefresherHandler>();

            builder.Components().Append<SeoToolkitDeployComponent>();
        }
    }
}
