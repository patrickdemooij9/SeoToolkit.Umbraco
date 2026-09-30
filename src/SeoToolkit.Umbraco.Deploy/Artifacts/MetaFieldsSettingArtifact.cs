using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.Artifacts
{
    public class MetaFieldsSettingArtifact(GuidUdi? udi, IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public GuidUdi? InheritanceUdi { get; set; }

        public List<MetaFieldsSettingField> Fields { get; set; } = [];
    }

    public class MetaFieldsSettingField
    {
        public required string Alias { get; set; }

        public bool UseInheritedValue { get; set; }

        /// <summary>Newtonsoft-serialized JSON of the field value (same wire format as the uSync serializer).</summary>
        public string? Value { get; set; }
    }
}
