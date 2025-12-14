using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.ArtifactModels
{
    public class RobotsTxtArtifact : DeployArtifactBase<GuidUdi>
    {
        public RobotsTxtArtifact(GuidUdi udi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, dependencies)
        { }

        public string Content { get; set; }
        public Guid? DomainId { get; set; }
    }
}
