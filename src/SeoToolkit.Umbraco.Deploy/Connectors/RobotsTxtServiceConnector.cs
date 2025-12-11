using SeoToolkit.Umbraco.Deploy.ArtifactModels;
using SeoToolkit.Umbraco.Deploy.Extensions;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors;
using static Umbraco.Cms.Core.Constants;

namespace SeoToolkit.Umbraco.Deploy.Connectors
{

    [UdiDefinition("seoToolkit-robotstxt", UdiType.GuidUdi)]
    public class RobotsTxtServiceConnector : ServiceConnectorBase<RobotsTxtArtifact, GuidUdi, ArtifactDeployState<RobotsTxtArtifact, RobotsTxtModel>>
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
                    yield return new GuidUdi("seoToolkit-robotstxt", item.Key);
                }
                yield break;
            }

            var entity = _robotsTxtService.Get(((GuidUdi)range.Udi).Guid);
            if (entity == null)
            {
                yield break;
            }

            yield return new GuidUdi("seoToolkit-robotstxt", entity.Key);
        }

        public override async Task<RobotsTxtArtifact?> GetArtifactAsync(GuidUdi udi, IContextCache contextCache, CancellationToken cancellationToken = default)
        {
            EnsureType(udi);

            var item = _robotsTxtService.Get(udi.Guid);
            if (item is null)
                return null;

            return new RobotsTxtArtifact(new GuidUdi("seoToolkit-robotstxt", item.Key))
            {
                DomainId = item.DomainId,
                Content = item.Content
            };
        }

        public override async Task<RobotsTxtArtifact> GetArtifactAsync(ArtifactDeployState<RobotsTxtArtifact, RobotsTxtModel> entity, IContextCache contextCache, CancellationToken cancellationToken = default)
        {
            if (entity.Entity is null)
                return null;

            return new RobotsTxtArtifact(new GuidUdi("seoToolkit-robotstxt", entity.Entity.Key))
            {
                DomainId = entity.Entity.DomainId,
                Content = entity.Entity.Content
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
            return new NamedUdiRange(robotsTxt.GetUdi(), selector);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(string entityType, string sid, string selector, CancellationToken cancellationToken = default)
        {
            EnsureType(entityType);
            if(sid == "-1")
            {
                EnsureOpenSelector(selector);
                return new NamedUdiRange(Udi.Create("seoToolkit-robotstxt"), OpenUdiName, selector);
            }

            if (!Guid.TryParse(sid, out Guid guidResult))
            {
                throw new ArgumentException("Invalid identifier", nameof(sid));
            }

            var entity = _robotsTxtService.Get(guidResult);
            if(entity == null)
            {
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));
            }

            return await GetRangeAsync(entity.GetUdi(), selector);
        }

        public override async Task ProcessAsync(ArtifactDeployState<RobotsTxtArtifact, ArtifactDeployState<RobotsTxtArtifact, RobotsTxtModel>> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task<ArtifactDeployState<RobotsTxtArtifact, ArtifactDeployState<RobotsTxtArtifact, RobotsTxtModel>>> ProcessInitAsync(RobotsTxtArtifact artifact, IDeployContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Difference> GetDifferences(RobotsTxtArtifact art1, RobotsTxtArtifact art2)
        {
            // Can do custom logic here to check for differences and put in a nicer message
            return base.GetDifferences(art1, art2);
        }
    }
}
