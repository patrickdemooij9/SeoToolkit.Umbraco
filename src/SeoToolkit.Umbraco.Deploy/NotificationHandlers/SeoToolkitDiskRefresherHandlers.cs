using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Disk;

namespace SeoToolkit.Umbraco.Deploy.NotificationHandlers
{
    /// <summary>
    /// Shared logic for the disk (.uda) refresher handlers: resolve the matching service
    /// connector, build the artifact and write it to (or delete it from) disk.
    /// </summary>
    public abstract class SeoToolkitDiskRefresherHandlerBase(
        IDiskEntityService diskEntityService,
        IServiceConnectorFactory serviceConnectorFactory)
    {
        protected async Task WriteArtifactAsync(string entityType, Guid id, CancellationToken cancellationToken)
        {
            var udi = new GuidUdi(entityType, id);
            IServiceConnector connector = serviceConnectorFactory.GetConnector(udi.EntityType);
            IArtifact? artifact = await connector.GetArtifactAsync(udi, PassThroughCache.Instance, cancellationToken)
                .ConfigureAwait(false);
            if (artifact is not null)
            {
                await diskEntityService.WriteArtifactsAsync([artifact], cancellationToken).ConfigureAwait(false);
            }
        }

        protected void DeleteArtifact(string entityType, Guid id)
        {
            var udi = new GuidUdi(entityType, id);
            diskEntityService.DeleteArtifacts([new DeletionArtifact(udi)]);
        }

        /// <summary>Minimal artifact used only to carry a UDI to <see cref="IDiskEntityService.DeleteArtifacts"/>.</summary>
        private sealed class DeletionArtifact(GuidUdi udi) : DeployArtifactBase<GuidUdi>(udi);
    }

    public class SeoSettingDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SeoSettingSavedNotification>
    {
        public Task HandleAsync(SeoSettingSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, notification.ContentTypeGuid, cancellationToken);
    }

    public class MetaFieldsSettingDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<MetaFieldSettingsSavedNotification>
    {
        public Task HandleAsync(MetaFieldSettingsSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, notification.Model.Content.Key, cancellationToken);
    }

    public class SitemapPageTypeDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SitemapPageSettingsSavedNotification>
    {
        public Task HandleAsync(SitemapPageSettingsSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, notification.Model.ContentTypeGuid, cancellationToken);
    }

    public class ScriptDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<ScriptSavedNotification>,
          INotificationAsyncHandler<ScriptDeletedNotification>
    {
        public Task HandleAsync(ScriptSavedNotification notification, CancellationToken cancellationToken)
            => notification.Script.Key is null
                ? Task.CompletedTask
                : WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.Script, notification.Script.Key.Value, cancellationToken);

        public Task HandleAsync(ScriptDeletedNotification notification, CancellationToken cancellationToken)
        {
            DeleteArtifact(SeoToolkitDeployConstants.UdiEntityType.Script, notification.Key);
            return Task.CompletedTask;
        }
    }

    public class DomainCollectionDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SeoDomainCollectionSavedNotification>,
          INotificationAsyncHandler<SeoDomainCollectionDeletedNotification>
    {
        public Task HandleAsync(SeoDomainCollectionSavedNotification notification, CancellationToken cancellationToken)
            => notification.Collection.Id is null
                ? Task.CompletedTask
                : WriteArtifactAsync(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, notification.Collection.Id.Value, cancellationToken);

        public Task HandleAsync(SeoDomainCollectionDeletedNotification notification, CancellationToken cancellationToken)
        {
            DeleteArtifact(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, notification.Id);
            // The collection's key/values .uda is keyed by the collection id and declares an Exist
            // dependency on the collection; delete it too so it isn't left orphaned on disk (which
            // would break a later disk deployment or resurrect the deleted collection's key/values).
            DeleteArtifact(SeoToolkitDeployConstants.UdiEntityType.KeyValues, notification.Id);
            return Task.CompletedTask;
        }
    }

    public class KeyValuesDiskRefresherHandler(
        IDiskEntityService diskEntityService, IServiceConnectorFactory serviceConnectorFactory)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SeoKeyValueSavedNotification>
    {
        public Task HandleAsync(SeoKeyValueSavedNotification notification, CancellationToken cancellationToken)
            => WriteArtifactAsync(
                SeoToolkitDeployConstants.UdiEntityType.KeyValues,
                notification.DomainCollectionId ?? SeoToolkitDeployConstants.RootKeyValuesGuid,
                cancellationToken);
    }

    /// <summary>
    /// Keeps the per-node MetaFields values .uda in sync with the database: (re)writes it while
    /// the node still has values, and deletes it once the node has none left — so removing all of
    /// a node's SEO values propagates as a delete on restore instead of leaving stale target data.
    /// </summary>
    public class MetaFieldsValueDiskRefresherHandler(
        IDiskEntityService diskEntityService,
        IServiceConnectorFactory serviceConnectorFactory,
        IMetaFieldsValueRepository valueRepository)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<MetaFieldsValueChangedNotification>
    {
        public Task HandleAsync(MetaFieldsValueChangedNotification notification, CancellationToken cancellationToken)
        {
            if (valueRepository.HasAnyValues(notification.NodeKey))
            {
                return WriteArtifactAsync(
                    SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, notification.NodeKey, cancellationToken);
            }

            DeleteArtifact(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, notification.NodeKey);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Keeps the per-node sitemap content .uda in sync with the database: (re)writes it while the
    /// node still has non-default settings, and deletes it once the settings are reset to default
    /// (the service deletes the row) — so a reset propagates as a delete on restore instead of
    /// leaving stale target data.
    /// </summary>
    public class SitemapContentDiskRefresherHandler(
        IDiskEntityService diskEntityService,
        IServiceConnectorFactory serviceConnectorFactory,
        ISitemapService sitemapService)
        : SeoToolkitDiskRefresherHandlerBase(diskEntityService, serviceConnectorFactory),
          INotificationAsyncHandler<SitemapContentChangedNotification>
    {
        public Task HandleAsync(SitemapContentChangedNotification notification, CancellationToken cancellationToken)
        {
            if (sitemapService.GetContentSettings(notification.NodeKey) is not null)
            {
                return WriteArtifactAsync(
                    SeoToolkitDeployConstants.UdiEntityType.SitemapContent, notification.NodeKey, cancellationToken);
            }

            DeleteArtifact(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, notification.NodeKey);
            return Task.CompletedTask;
        }
    }
}
