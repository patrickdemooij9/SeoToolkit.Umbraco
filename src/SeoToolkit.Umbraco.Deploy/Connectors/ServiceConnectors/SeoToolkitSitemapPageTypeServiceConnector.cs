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
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, UdiType.GuidUdi)]
    public class SeoToolkitSitemapPageTypeServiceConnector(
        ISitemapService sitemapService,
        IContentTypeService contentTypeService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<SitemapPageTypeArtifact, SitemapPageSettings>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.SitemapPageType;

        protected override string OpenUdiName => "All SeoToolkit sitemap page type settings";

        protected override int[] ProcessPasses => [2];

        public override string GetEntityName(SitemapPageSettings entity)
            => contentTypeService.Get(entity.ContentTypeGuid)?.Name ?? entity.ContentTypeGuid.ToString();

        protected override GuidUdi GetEntityUdi(SitemapPageSettings entity)
            => new(UdiEntityType, entity.ContentTypeGuid);

        public override Task<SitemapPageSettings?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(sitemapService.GetPageTypeSettings(id));

        public override async IAsyncEnumerable<SitemapPageSettings> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in sitemapService.GetAll())
            {
                yield return entity;
            }
        }

        public override Task<SitemapPageTypeArtifact?> GetArtifactAsync(
            GuidUdi? udi, SitemapPageSettings? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<SitemapPageTypeArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.DocumentType, entity.ContentTypeGuid)),
            };

            return Task.FromResult<SitemapPageTypeArtifact?>(new SitemapPageTypeArtifact(udi, dependencies)
            {
                Name = GetEntityName(entity),
                HideFromSitemap = entity.HideFromSitemap,
                ChangeFrequency = entity.ChangeFrequency,
                Priority = entity.Priority,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<SitemapPageTypeArtifact, SitemapPageSettings> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var contentTypeKey = state.Artifact.Udi.Guid;
            if (contentTypeService.Get(contentTypeKey) is null)
            {
                return Task.CompletedTask;
            }

            sitemapService.SetPageTypeSettings(new SitemapPageSettings
            {
                ContentTypeGuid = contentTypeKey,
                HideFromSitemap = state.Artifact.HideFromSitemap,
                ChangeFrequency = state.Artifact.ChangeFrequency ?? string.Empty,
                Priority = state.Artifact.Priority,
            });
            return Task.CompletedTask;
        }
    }
}
