using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.Script, UdiType.GuidUdi)]
    public class SeoToolkitScriptServiceConnector(
        IScriptManagerService scriptManagerService,
        ScriptDefinitionCollection scriptDefinitions,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<ScriptArtifact, Script>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.Script;

        protected override string OpenUdiName => "All SeoToolkit scripts";

        protected override int[] ProcessPasses => [2];

        public override string GetEntityName(Script entity) => entity.Name;

        protected override GuidUdi GetEntityUdi(Script entity)
            => new(UdiEntityType, entity.Key ?? Guid.Empty);

        public override Task<Script?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(scriptManagerService.Get(id));

        public override async IAsyncEnumerable<Script> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            // GetAll is per-domain; enumerate the no-domain scripts plus every known script by key.
            foreach (var script in scriptManagerService.GetAll(null))
            {
                yield return script;
            }
        }

        public override Task<ScriptArtifact?> GetArtifactAsync(
            GuidUdi? udi, Script? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<ScriptArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection();
            GuidUdi? domainCollectionUdi = null;
            if (entity.DomainId is not null)
            {
                domainCollectionUdi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, entity.DomainId.Value);
                dependencies.Add(new SeoToolkitArtifactDependency(domainCollectionUdi));
            }

            return Task.FromResult<ScriptArtifact?>(new ScriptArtifact(udi, dependencies)
            {
                Name = entity.Name,
                DefinitionAlias = entity.Definition.Alias,
                Config = entity.Config ?? [],
                DomainCollectionUdi = domainCollectionUdi,
                SortOrder = entity.SortOrder,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<ScriptArtifact, Script> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var definition = scriptDefinitions.FirstOrDefault(d => d.Alias == state.Artifact.DefinitionAlias);
            if (definition is null)
            {
                return Task.CompletedTask; // script definition type not installed in target; skip
            }

            var script = state.Entity ?? new Script();
            script.Key = state.Artifact.Udi.Guid;
            script.Name = state.Artifact.Name;
            script.Definition = definition;
            script.Config = state.Artifact.Config;
            script.DomainId = state.Artifact.DomainCollectionUdi?.Guid;
            script.SortOrder = state.Artifact.SortOrder;

            scriptManagerService.Save(script);
            return Task.CompletedTask;
        }
    }
}
