using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;

namespace SeoToolkit.Umbraco.Deploy.NotificationHandlers
{
    /// <summary>
    /// Shared logic for the per-node signature refreshers: keep the Deploy signature current on
    /// change (signatures only, no disk .uda) so an edit is detected as a change to transfer.
    /// </summary>
    public abstract class SeoToolkitSignatureRefresherHandlerBase(
        IServiceConnectorFactory serviceConnectorFactory,
        ISignatureService signatureService)
    {
        protected async Task RefreshSignatureAsync(string entityType, Guid id, CancellationToken cancellationToken)
        {
            var udi = new GuidUdi(entityType, id);
            IServiceConnector connector = serviceConnectorFactory.GetConnector(udi.EntityType);
            IArtifact? artifact = await connector.GetArtifactAsync(udi, PassThroughCache.Instance, cancellationToken)
                .ConfigureAwait(false);
            if (artifact is not null)
            {
                signatureService.SetSignature(artifact);
            }
            else
            {
                // No values left for the node — drop the stale signature.
                signatureService.ClearSignature(udi);
            }
        }
    }

    public class MetaFieldsValueSignatureRefresherHandler(
        IServiceConnectorFactory serviceConnectorFactory, ISignatureService signatureService)
        : SeoToolkitSignatureRefresherHandlerBase(serviceConnectorFactory, signatureService),
          INotificationAsyncHandler<MetaFieldsValueChangedNotification>
    {
        public Task HandleAsync(MetaFieldsValueChangedNotification notification, CancellationToken cancellationToken)
            => RefreshSignatureAsync(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, notification.NodeKey, cancellationToken);
    }

    public class SitemapContentSignatureRefresherHandler(
        IServiceConnectorFactory serviceConnectorFactory, ISignatureService signatureService)
        : SeoToolkitSignatureRefresherHandlerBase(serviceConnectorFactory, signatureService),
          INotificationAsyncHandler<SitemapContentChangedNotification>
    {
        public Task HandleAsync(SitemapContentChangedNotification notification, CancellationToken cancellationToken)
            => RefreshSignatureAsync(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, notification.NodeKey, cancellationToken);
    }
}
