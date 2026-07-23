using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, UdiType.GuidUdi)]
    public class SeoToolkitMetaFieldsValueServiceConnector(
        IMetaFieldsValueRepository valueRepository,
        IContentService contentService,
        SeoFieldCollection seoFieldCollection,
        IMetaFieldsValueService metaFieldsValueService,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<MetaFieldsValueArtifact, MetaFieldsNodeValuesModel>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue;

        protected override string OpenUdiName => "All SeoToolkit meta field values";

        protected override int[] ProcessPasses => [7];

        public override string GetEntityName(MetaFieldsNodeValuesModel entity) => entity.NodeName;

        protected override GuidUdi GetEntityUdi(MetaFieldsNodeValuesModel entity)
            => new(UdiEntityType, entity.NodeKey);

        public override Task<MetaFieldsNodeValuesModel?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var values = valueRepository.GetAllValues(id);
            if (values.Count == 0)
            {
                return Task.FromResult<MetaFieldsNodeValuesModel?>(null);
            }

            var content = contentService.GetById(id);
            return Task.FromResult<MetaFieldsNodeValuesModel?>(
                content is null ? null : new MetaFieldsNodeValuesModel(id, content.Name ?? id.ToString(), values));
        }

        public override async IAsyncEnumerable<MetaFieldsNodeValuesModel> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var nodeKey in valueRepository.GetAllNodeKeys())
            {
                var entity = await GetEntityAsync(nodeKey, cancellationToken).ConfigureAwait(false);
                if (entity is not null)
                {
                    yield return entity;
                }
            }
        }

        public override Task<MetaFieldsValueArtifact?> GetArtifactAsync(
            GuidUdi? udi, MetaFieldsNodeValuesModel? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<MetaFieldsValueArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.Document, entity.NodeKey)),
            };

            // Order cultures and aliases (ordinal) so identical data always serializes the same
            // way — the artifact is attached to its document as a Match dependency, so an order
            // flip would otherwise make the document look out of date and re-transfer for nothing.
            var values = new Dictionary<string, Dictionary<string, string?>>();
            foreach (var (culture, fields) in entity.Values.OrderBy(c => c.Key, StringComparer.Ordinal))
            {
                var cultureValues = new Dictionary<string, string?>();
                foreach (var (alias, value) in fields.OrderBy(f => f.Key, StringComparer.Ordinal))
                {
                    var json = value is null ? null : JsonConvert.SerializeObject(value);

                    // UDIs embedded in the value JSON (e.g. RTE-like content).
                    foreach (var referencedUdi in UdiJsonHelper.FindUdis(json))
                    {
                        dependencies.Add(new SeoToolkitArtifactDependency(referencedUdi));
                    }

                    // GUID-based media references: the value is a bare media key (database form),
                    // so ask the field's converter which media it points at.
                    if (value is not null
                        && seoFieldCollection.Get(alias)?.Editor.ValueConverter is IMediaReferenceConverter mediaConverter)
                    {
                        foreach (var mediaKey in mediaConverter.GetReferencedMediaKeys(value))
                        {
                            dependencies.Add(new SeoToolkitArtifactDependency(
                                new GuidUdi(Constants.UdiEntityType.Media, mediaKey)));
                        }
                    }

                    cultureValues[alias] = json;
                }

                values[culture] = cultureValues;
            }

            return Task.FromResult<MetaFieldsValueArtifact?>(new MetaFieldsValueArtifact(udi, dependencies)
            {
                Name = entity.NodeName,
                Values = values,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<MetaFieldsValueArtifact, MetaFieldsNodeValuesModel> state,
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
                return Task.CompletedTask; // node not (yet) in target; skip rather than fail
            }

            // The artifact is the node's complete value set: delete target values absent from the
            // source (an absent or null value is a clear) so it converges.
            foreach (var (culture, fields) in valueRepository.GetAllValues(nodeKey))
            {
                foreach (var alias in fields.Keys)
                {
                    var inArtifact = state.Artifact.Values.TryGetValue(culture, out var artifactFields)
                        && artifactFields.TryGetValue(alias, out var artifactJson)
                        && artifactJson is not null;
                    if (!inArtifact)
                    {
                        valueRepository.Delete(nodeKey, alias, culture);
                    }
                }
            }

            foreach (var (culture, fields) in state.Artifact.Values)
            {
                foreach (var (alias, json) in fields)
                {
                    if (json is null)
                    {
                        continue;
                    }

                    var value = JsonConvert.DeserializeObject(json);
                    if (value is null)
                    {
                        continue;
                    }

                    if (valueRepository.Exists(nodeKey, alias, culture))
                    {
                        valueRepository.Update(nodeKey, alias, culture, value);
                    }
                    else
                    {
                        valueRepository.Add(nodeKey, alias, culture, value);
                    }
                }
            }

            // We wrote straight to the repository (rather than through the service's culture-aware
            // AddValues), so ask the service to invalidate the per-node runtime cache across the
            // load-balanced environment and publish the change notification — which also refreshes
            // the target's own per-node .uda via MetaFieldsValueDiskRefresherHandler.
            metaFieldsValueService.NotifyChanged(nodeKey);

            return Task.CompletedTask;
        }
    }
}
