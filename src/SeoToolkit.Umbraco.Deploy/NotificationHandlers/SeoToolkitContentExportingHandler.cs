using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core.Events;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;

namespace SeoToolkit.Umbraco.Deploy.NotificationHandlers
{
    /// <summary>
    /// When a document artifact is about to be exported (transfer, restore source, queue for
    /// transfer), appends the node's SeoToolkit per-node artifacts as Match dependencies so they
    /// are pulled into the same deployment automatically and re-transferred whenever the SEO data
    /// changes.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ArtifactExportingNotification"/> (fires before the artifact is serialized) —
    /// <see cref="ArtifactExportedNotification"/> fires after serialization, so dependencies added
    /// there would never reach the exported artifact.
    /// </remarks>
    public class SeoToolkitContentExportingHandler(
        IMetaFieldsValueRepository valueRepository,
        ISitemapService sitemapService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : INotificationAsyncHandler<ArtifactExportingNotification>
    {
        private bool IsDisabled(string entityType)
            => settings.CurrentValue.DisabledEntityTypes.Contains(entityType, StringComparer.OrdinalIgnoreCase);

        public Task HandleAsync(ArtifactExportingNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Artifact is not DocumentArtifact documentArtifact
                || documentArtifact.Udi is not GuidUdi documentUdi)
            {
                return Task.CompletedTask;
            }

            var extraDependencies = new List<ArtifactDependency>();

            // Match (not Exist) mode so Deploy compares the per-node artifact's checksum and
            // re-transfers it when the SEO data changes — Exist would only ensure a value row is
            // present on the target and would leave stale values behind after the first transfer.
            // Skip the dependency when the connector is disabled: it returns no artifact, so a
            // Match dependency on it could never be satisfied and would fail the document transfer.
            if (!IsDisabled(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue)
                && valueRepository.HasAnyValues(documentUdi.Guid))
            {
                extraDependencies.Add(new SeoToolkitArtifactDependency(
                    new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, documentUdi.Guid),
                    ArtifactDependencyMode.Match));
            }

            if (!IsDisabled(SeoToolkitDeployConstants.UdiEntityType.SitemapContent)
                && sitemapService.GetContentSettings(documentUdi.Guid) is not null)
            {
                extraDependencies.Add(new SeoToolkitArtifactDependency(
                    new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, documentUdi.Guid),
                    ArtifactDependencyMode.Match));
            }

            if (extraDependencies.Count > 0)
            {
                // The setter re-orders by Udi; safe to reassign.
                documentArtifact.Dependencies = documentArtifact.Dependencies.Concat(extraDependencies).ToList();
            }

            return Task.CompletedTask;
        }
    }
}
