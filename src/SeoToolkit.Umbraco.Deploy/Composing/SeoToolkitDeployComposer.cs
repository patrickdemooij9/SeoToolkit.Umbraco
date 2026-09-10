using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<SeoToolkitDeploySettings>()
                .Bind(builder.Config.GetSection("SeoToolkit:Deploy"));

            // Disk (.uda) refreshers: rewrite the settings artifact whenever it is saved/deleted.
            builder.AddNotificationAsyncHandler<SeoSettingSavedNotification, SeoSettingDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<MetaFieldSettingsSavedNotification, MetaFieldsSettingDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SitemapPageSettingsSavedNotification, SitemapPageTypeDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<ScriptSavedNotification, ScriptDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<ScriptDeletedNotification, ScriptDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoDomainCollectionSavedNotification, DomainCollectionDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoDomainCollectionDeletedNotification, DomainCollectionDiskRefresherHandler>();
            builder.AddNotificationAsyncHandler<SeoKeyValueSavedNotification, KeyValuesDiskRefresherHandler>();

            // Per-node types: refresh the Deploy signature on change so an edit is detected as a
            // change to transfer (they write no disk .uda).
            builder.AddNotificationAsyncHandler<MetaFieldsValueChangedNotification, MetaFieldsValueSignatureRefresherHandler>();
            builder.AddNotificationAsyncHandler<SitemapContentChangedNotification, SitemapContentSignatureRefresherHandler>();

            builder.Components().Append<SeoToolkitDeployComponent>();
        }
    }
}
