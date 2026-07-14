using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, UdiType.GuidUdi)]
    public class SeoToolkitSeoSettingServiceConnector(
        ISeoSettingsService seoSettingsService,
        IContentTypeService contentTypeService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<SeoSettingArtifact, SeoSettingModel>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.SeoSetting;

        protected override string OpenUdiName => "All SeoToolkit SEO settings";

        protected override int[] ProcessPasses => [2];

        public override string GetEntityName(SeoSettingModel entity) => entity.ContentTypeName;

        protected override GuidUdi GetEntityUdi(SeoSettingModel entity)
            => new(UdiEntityType, entity.ContentTypeKey);

        public override Task<SeoSettingModel?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!seoSettingsService.GetAll().TryGetValue(id, out var enabled))
            {
                return Task.FromResult<SeoSettingModel?>(null);
            }

            var contentType = contentTypeService.Get(id);
            return Task.FromResult<SeoSettingModel?>(
                contentType is null ? null : new SeoSettingModel(id, enabled, contentType.Name ?? contentType.Alias));
        }

        public override async IAsyncEnumerable<SeoSettingModel> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var (key, _) in seoSettingsService.GetAll())
            {
                var entity = await GetEntityAsync(key, cancellationToken).ConfigureAwait(false);
                if (entity is not null)
                {
                    yield return entity;
                }
            }
        }

        public override Task<SeoSettingArtifact?> GetArtifactAsync(
            GuidUdi? udi, SeoSettingModel? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<SeoSettingArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.DocumentType, entity.ContentTypeKey)),
            };

            return Task.FromResult<SeoSettingArtifact?>(new SeoSettingArtifact(udi, dependencies)
            {
                Name = entity.ContentTypeName,
                Enabled = entity.Enabled,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<SeoSettingArtifact, SeoSettingModel> state,
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
                // Target environment doesn't have the document type; skip rather than fail the deploy.
                return Task.CompletedTask;
            }

            seoSettingsService.ToggleSeoSettings(contentTypeKey, state.Artifact.Enabled);
            return Task.CompletedTask;
        }
    }
}
