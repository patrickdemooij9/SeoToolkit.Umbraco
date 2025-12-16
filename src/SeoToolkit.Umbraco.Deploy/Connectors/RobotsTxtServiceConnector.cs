using SeoToolkit.Umbraco.Deploy.ArtifactModels;
using SeoToolkit.Umbraco.Deploy.Extensions;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Startup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors;
using static Umbraco.Cms.Core.Constants;

namespace SeoToolkit.Umbraco.Deploy.Connectors
{

    [UdiDefinition(SeoToolkitDeployConstants.UdiRobotsTxtEntityType, UdiType.GuidUdi)]
    public class RobotsTxtServiceConnector : ServiceConnectorBase<RobotsTxtArtifact, GuidUdi, RobotsTxtModel>
    {
        private readonly IRobotsTxtService _robotsTxtService;

        public RobotsTxtServiceConnector(IRobotsTxtService robotsTxtService)
        {
            _robotsTxtService = robotsTxtService;
        }

        protected override string[] ValidOpenSelectors => new[]
        {
            DeploySelector.This,
            DeploySelector.ThisAndDescendants, // Not sure we need this
            DeploySelector.DescendantsOfThis // Not sure we need this
        };

        protected override string OpenUdiName => "all-robots-txt";

        protected override int[] ProcessPasses => [2];

        public override async IAsyncEnumerable<GuidUdi> ExpandRangeAsync(UdiRange range, CancellationToken cancellationToken = default)
        {
            EnsureType(range.Udi);
            if (range.Udi.IsRoot)
            {
                foreach(var item in _robotsTxtService.GetAll())
                {
                    yield return new GuidUdi(SeoToolkitDeployConstants.UdiRobotsTxtEntityType, item.Key);
                }
                yield break;
            }

            var entity = _robotsTxtService.Get(((GuidUdi)range.Udi).Guid);
            if (entity == null)
            {
                yield break;
            }

            yield return new GuidUdi(SeoToolkitDeployConstants.UdiRobotsTxtEntityType, entity.Key);
        }

        public override async Task<RobotsTxtArtifact?> GetArtifactAsync(GuidUdi udi, IContextCache contextCache, CancellationToken cancellationToken = default)
        {
            EnsureType(udi);

            var item = _robotsTxtService.Get(udi.Guid);
            if (item is null)
                return null;

            return new RobotsTxtArtifact(new GuidUdi(SeoToolkitDeployConstants.UdiRobotsTxtEntityType, item.Key))
            {
                Id = item.Id,
                DomainId = item.DomainId,
                Content = item.Content
            };
        }

        public override async Task<RobotsTxtArtifact> GetArtifactAsync(RobotsTxtModel entity, IContextCache contextCache, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return null;

            return new RobotsTxtArtifact(new GuidUdi(SeoToolkitDeployConstants.UdiRobotsTxtEntityType, entity.Key))
            {
                Id = entity.Id,
                DomainId = entity.DomainId,
                Content = entity.Content
            };
        }

        public override async Task<NamedUdiRange> GetRangeAsync(GuidUdi udi, string selector, CancellationToken cancellationToken = default)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(udi, selector);
                return new NamedUdiRange(udi, OpenUdiName, selector);
            }

            var robotsTxt = _robotsTxtService.Get(udi.Guid);
            return new NamedUdiRange(robotsTxt.GetUdi(), "Robots.Txt", selector);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(string entityType, string sid, string selector, CancellationToken cancellationToken = default)
        {
            EnsureType(entityType);

            // Check if we have the root robots.txt node
            if (Guid.TryParse(sid, out var resultGuid) && resultGuid == RobotsTxtTreeSection.RobotsTxtSectionGuid)
            {
                var entity = _robotsTxtService.GetAll().FirstOrDefault(it => it.DomainId is null);
                return entity is null
                    ? throw new ArgumentException("Could not find entity")
                    : await GetRangeAsync(entity.GetUdi(), selector, cancellationToken);
            }

            throw new ArgumentException("Invalid identifier", nameof(sid));
        }

        public override Task ProcessAsync(ArtifactDeployState<RobotsTxtArtifact, RobotsTxtModel> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            var model = new RobotsTxtModel
            {
                Id = state.Artifact.Id,
                Key = state.Artifact.Udi.Guid,
                Content = state.Artifact.Content,
                DomainId = state.Artifact.DomainId
            };
            _robotsTxtService.Save(model);
            return Task.CompletedTask;
        }

        public override Task<ArtifactDeployState<RobotsTxtArtifact, RobotsTxtModel>> ProcessInitAsync(RobotsTxtArtifact artifact, IDeployContext context, CancellationToken cancellationToken = default)
        {
            var entity = _robotsTxtService.Get(artifact.Udi.Guid);

            return Task.FromResult(ArtifactDeployState.Create(artifact, entity, this, ProcessPasses[0]));
        }

        protected override IEnumerable<Difference> GetDifferences(RobotsTxtArtifact art1, RobotsTxtArtifact art2)
        {
            // Can do custom logic here to check for differences and put in a nicer message
            return base.GetDifferences(art1, art2);
        }
    }
}
