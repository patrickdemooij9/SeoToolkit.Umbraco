using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, UdiType.GuidUdi)]
    public class SeoToolkitSitemapContentServiceConnector(
        ISitemapService sitemapService,
        IContentService contentService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<SitemapContentArtifact, SitemapContentSettings>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.SitemapContent;

        protected override string OpenUdiName => "All SeoToolkit sitemap content settings";

        protected override int[] ProcessPasses => [7];

        public override string GetEntityName(SitemapContentSettings entity)
            => contentService.GetById(entity.NodeKey)?.Name ?? entity.NodeKey.ToString();

        protected override GuidUdi GetEntityUdi(SitemapContentSettings entity)
            => new(UdiEntityType, entity.NodeKey);

        public override Task<SitemapContentSettings?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(sitemapService.GetContentSettings(id));

        public override async IAsyncEnumerable<SitemapContentSettings> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in sitemapService.GetAllContentSettings())
            {
                yield return entity;
            }
        }

        public override Task<SitemapContentArtifact?> GetArtifactAsync(
            GuidUdi? udi, SitemapContentSettings? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<SitemapContentArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.Document, entity.NodeKey)),
            };

            return Task.FromResult<SitemapContentArtifact?>(new SitemapContentArtifact(udi, dependencies)
            {
                Name = GetEntityName(entity),
                ExcludeFromSitemap = entity.ExcludeFromSitemap,
                ChangeFrequency = entity.ChangeFrequency,
                Priority = entity.Priority,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<SitemapContentArtifact, SitemapContentSettings> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 7 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var nodeKey = state.Artifact.Udi.Guid;
            if (contentService.GetById(nodeKey) is null)
            {
                return Task.CompletedTask;
            }

            sitemapService.SetContentSettings(new SitemapContentSettings
            {
                NodeKey = nodeKey,
                ExcludeFromSitemap = state.Artifact.ExcludeFromSitemap,
                ChangeFrequency = state.Artifact.ChangeFrequency ?? string.Empty,
                Priority = state.Artifact.Priority,
            });
            return Task.CompletedTask;
        }
    }
}
