using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace SeoToolkit.Umbraco.Deploy.ArtifactModels
{
    internal class SitemapSettingsArtifact : DeployArtifactBase<GuidUdi>
    {
        public required bool HideFromSitemap { get; set; }
        public required string ChangeFrequency { get; set; }
        public double? Priority { get; set; }

        public SitemapSettingsArtifact(GuidUdi udi, IEnumerable<ArtifactDependency>? dependencies = null) : base(udi, dependencies)
        {
        }
    }
}
