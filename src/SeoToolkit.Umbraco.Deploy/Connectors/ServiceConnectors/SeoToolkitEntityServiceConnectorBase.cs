using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    public abstract class SeoToolkitEntityServiceConnectorBase<TArtifact, TEntity>(
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : ServiceConnectorBase<TArtifact, GuidUdi, TEntity>
        where TArtifact : DeployArtifactBase<GuidUdi>
        where TEntity : class
    {
        public abstract string UdiEntityType { get; }

        protected override string[] ValidOpenSelectors => ["this", "this-and-descendants", "descendants"];

        protected bool IsDisabled
            => settings.CurrentValue.DisabledEntityTypes.Contains(UdiEntityType, StringComparer.OrdinalIgnoreCase);

        protected bool PruneMissing => settings.CurrentValue.PruneMissing;

        public abstract string GetEntityName(TEntity entity);

        protected abstract GuidUdi GetEntityUdi(TEntity entity);

        public abstract Task<TEntity?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default);

        public abstract IAsyncEnumerable<TEntity> GetEntitiesAsync(CancellationToken cancellationToken = default);

        public abstract Task<TArtifact?> GetArtifactAsync(GuidUdi? udi, TEntity? entity, CancellationToken cancellationToken = default);

        public override Task<TArtifact> GetArtifactAsync(
            TEntity entity,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
            => GetArtifactAsync(GetEntityUdi(entity), entity, cancellationToken)!;

        public override async Task<TArtifact?> GetArtifactAsync(
            GuidUdi? udi,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            EnsureType(udi);
            if (IsDisabled)
            {
                return null;
            }

            TEntity? entity = await GetEntityAsync(udi.Guid, cancellationToken).ConfigureAwait(false);
            return entity == null ? null : await GetArtifactAsync(udi, entity, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(
            GuidUdi udi,
            string selector,
            CancellationToken cancellationToken = default)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(udi, selector);
                return new NamedUdiRange(udi, OpenUdiName, selector);
            }

            TEntity? entity = await GetEntityAsync(udi.Guid, cancellationToken).ConfigureAwait(false);
            if (entity == null)
            {
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));
            }

            return new NamedUdiRange(GetEntityUdi(entity), GetEntityName(entity), selector);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(
            string entityType,
            string sid,
            string selector,
            CancellationToken cancellationToken = default)
        {
            if (sid == "-1")
            {
                EnsureOpenSelector(selector);
                return new NamedUdiRange(Udi.Create(UdiEntityType), OpenUdiName, selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
            {
                throw new ArgumentException("Invalid identifier.", nameof(sid));
            }

            TEntity? entity = await GetEntityAsync(result, cancellationToken).ConfigureAwait(false);
            if (entity == null)
            {
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));
            }

            return new NamedUdiRange(GetEntityUdi(entity), GetEntityName(entity), selector);
        }

        public override async IAsyncEnumerable<GuidUdi?> ExpandRangeAsync(
            UdiRange range,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureType(range.Udi);
            if (IsDisabled)
            {
                yield break;
            }

            if (range.Udi.IsRoot)
            {
                EnsureSelector(range.Udi, range.Selector);

                await foreach (TEntity entity in GetEntitiesAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield return GetEntityUdi(entity);
                }
            }
            else
            {
                TEntity? entity = await GetEntityAsync(((GuidUdi)range.Udi).Guid, cancellationToken).ConfigureAwait(false);
                if (entity == null)
                {
                    yield break;
                }

                if (range.Selector != "this")
                {
                    throw new NotSupportedException("Unexpected selector \"" + range.Selector + "\".");
                }

                yield return GetEntityUdi(entity);
            }
        }

        public override async Task<ArtifactDeployState<TArtifact, TEntity>> ProcessInitAsync(
            TArtifact artifact,
            IDeployContext context,
            CancellationToken cancellationToken = default)
        {
            EnsureType(artifact.Udi);

            TEntity? entity = await GetEntityAsync(artifact.Udi.Guid, cancellationToken).ConfigureAwait(false);

            return ArtifactDeployState.Create(artifact, entity, this, ProcessPasses[0]);
        }
    }
}
