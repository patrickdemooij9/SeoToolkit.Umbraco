using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.KeyValues, UdiType.GuidUdi)]
    public class SeoToolkitKeyValuesServiceConnector(
        ISeoKeyValueRepository keyValueRepository,
        ISeoDomainsService seoDomainsService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<KeyValuesArtifact, KeyValuesModel>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.KeyValues;

        protected override string OpenUdiName => "All SeoToolkit key/values";

        protected override int[] ProcessPasses => [2];

        public override string GetEntityName(KeyValuesModel entity) => entity.Name;

        protected override GuidUdi GetEntityUdi(KeyValuesModel entity)
            => new(UdiEntityType, entity.ArtifactGuid);

        public override Task<KeyValuesModel?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == SeoToolkitDeployConstants.RootKeyValuesGuid)
            {
                var rootValues = keyValueRepository.Get((Guid?)null);
                return Task.FromResult<KeyValuesModel?>(
                    new KeyValuesModel(id, null, rootValues, "SeoToolkit key/values (global)"));
            }

            var collection = seoDomainsService.Get(id);
            if (collection is null)
            {
                return Task.FromResult<KeyValuesModel?>(null);
            }

            var values = keyValueRepository.Get(id);
            return Task.FromResult<KeyValuesModel?>(
                new KeyValuesModel(id, id, values, $"SeoToolkit key/values ({collection.Name})"));
        }

        public override async IAsyncEnumerable<KeyValuesModel> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var root = await GetEntityAsync(SeoToolkitDeployConstants.RootKeyValuesGuid, cancellationToken).ConfigureAwait(false);
            if (root is not null && root.Values.Count > 0)
            {
                yield return root;
            }

            foreach (var collection in seoDomainsService.GetAll())
            {
                if (collection.Id is null)
                {
                    continue;
                }

                var entity = await GetEntityAsync(collection.Id.Value, cancellationToken).ConfigureAwait(false);
                if (entity is not null && entity.Values.Count > 0)
                {
                    yield return entity;
                }
            }
        }

        public override Task<KeyValuesArtifact?> GetArtifactAsync(
            GuidUdi? udi, KeyValuesModel? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<KeyValuesArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection();
            GuidUdi? domainCollectionUdi = null;
            if (entity.DomainCollectionId is not null)
            {
                domainCollectionUdi = new GuidUdi(
                    SeoToolkitDeployConstants.UdiEntityType.DomainCollection, entity.DomainCollectionId.Value);
                dependencies.Add(new SeoToolkitArtifactDependency(domainCollectionUdi));
            }

            return Task.FromResult<KeyValuesArtifact?>(new KeyValuesArtifact(udi, dependencies)
            {
                Name = entity.Name,
                DomainCollectionUdi = domainCollectionUdi,
                Values = entity.Values,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<KeyValuesArtifact, KeyValuesModel> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            Guid? domainId = state.Artifact.DomainCollectionUdi?.Guid;
            foreach (var (key, value) in state.Artifact.Values)
            {
                keyValueRepository.Set(key, value, domainId);
            }

            return Task.CompletedTask;
        }
    }
}
