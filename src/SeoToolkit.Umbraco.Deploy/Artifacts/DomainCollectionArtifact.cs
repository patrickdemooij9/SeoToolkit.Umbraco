using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class DomainCollectionArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        /// <summary>Umbraco domain names (portable across environments, unlike int domain ids).</summary>
        public List<string> DomainNames { get; set; } = [];

        public Dictionary<string, string> Settings { get; set; } = [];
    }
}
