using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Umbraco.Deploy
{
    public class SeoToolkitArtifactDependency : ArtifactDependency
    {
        public SeoToolkitArtifactDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Exist)
            : base(udi, false, mode)
        {
        }
    }
}
