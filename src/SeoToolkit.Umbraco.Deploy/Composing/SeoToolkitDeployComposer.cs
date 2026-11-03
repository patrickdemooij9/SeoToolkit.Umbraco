using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core;
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

            // Without Umbraco Deploy the handlers and component below can't be constructed and the
            // site fails to start, so register nothing rather than crash.
            if (!UmbracoDeployDetector.IsInstalled(builder))
            {
                return;
            }

            // Deploy can read the .uda files before components initialize: an unattended upgrade only
            // initializes them once migrations finish. The types must be known by then, so register
            // them here rather than in the component.
            RegisterUdiTypes();

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

        private static void RegisterUdiTypes()
        {
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.Script, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.KeyValues, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, UdiType.GuidUdi);
        }
    }
}
