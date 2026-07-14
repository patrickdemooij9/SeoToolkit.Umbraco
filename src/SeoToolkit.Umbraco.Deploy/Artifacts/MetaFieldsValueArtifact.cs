using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class MetaFieldsValueArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        /// <summary>Culture (empty string = invariant) → field alias → Newtonsoft-serialized JSON value.</summary>
        public Dictionary<string, Dictionary<string, string?>> Values { get; set; } = [];
    }
}
