using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, UdiType.GuidUdi)]
    public class SeoToolkitDomainCollectionServiceConnector(
        ISeoDomainsService seoDomainsService,
        IDomainService domainService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<DomainCollectionArtifact, SeoDomainCollection>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.DomainCollection;

        protected override string OpenUdiName => "All SeoToolkit domain collections";

        protected override int[] ProcessPasses => [2];

        public override string GetEntityName(SeoDomainCollection entity) => entity.Name;

        protected override GuidUdi GetEntityUdi(SeoDomainCollection entity)
            => new(UdiEntityType, entity.Id ?? Guid.Empty);

        public override Task<SeoDomainCollection?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(seoDomainsService.Get(id));

        public override async IAsyncEnumerable<SeoDomainCollection> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in seoDomainsService.GetAll())
            {
                yield return entity;
            }
        }

        public override async Task<DomainCollectionArtifact?> GetArtifactAsync(
            GuidUdi? udi, SeoDomainCollection? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return null;
            }

            var allDomains = (await domainService.GetAllAsync(true).ConfigureAwait(false)).ToArray();
            var domainNames = entity.DomainIds
                .Select(id => allDomains.FirstOrDefault(d => d.Id == id)?.DomainName)
                .Where(name => name is not null)
                .Select(name => name!)
                .ToList();

            return new DomainCollectionArtifact(udi, new ArtifactDependencyCollection())
            {
                Name = entity.Name,
                DomainNames = domainNames,
                Settings = entity.Settings,
            };
        }

        public override async Task ProcessAsync(
            ArtifactDeployState<DomainCollectionArtifact, SeoDomainCollection> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return;
            }

            var allDomains = (await domainService.GetAllAsync(true).ConfigureAwait(false)).ToArray();
            var domainIds = state.Artifact.DomainNames
                .Select(name => allDomains.FirstOrDefault(d => string.Equals(d.DomainName, name, StringComparison.OrdinalIgnoreCase))?.Id)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            seoDomainsService.Save(new SeoDomainCollection
            {
                Id = state.Artifact.Udi.Guid,
                Name = state.Artifact.Name,
                DomainIds = domainIds,
                Settings = state.Artifact.Settings,
            });
        }
    }
}
