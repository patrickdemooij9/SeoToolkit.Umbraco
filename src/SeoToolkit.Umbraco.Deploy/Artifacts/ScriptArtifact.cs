using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class ScriptArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public required string DefinitionAlias { get; set; }

        public Dictionary<string, string> Config { get; set; } = [];

        public GuidUdi? DomainCollectionUdi { get; set; }

        public int SortOrder { get; set; }
    }
}
