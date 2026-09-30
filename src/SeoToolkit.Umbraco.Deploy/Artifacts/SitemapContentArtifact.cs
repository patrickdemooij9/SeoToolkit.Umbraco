using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class SitemapContentArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public bool ExcludeFromSitemap { get; set; }

        public string? ChangeFrequency { get; set; }

        public double? Priority { get; set; }
    }
}
