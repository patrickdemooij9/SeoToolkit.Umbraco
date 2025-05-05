using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

namespace SeoToolkit.Umbraco.NotFound;

internal class ManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest
        {
            PackageName = "SeoToolkit.Umbraco.NotFound",
            Version = "3.7.1",
            Scripts = new[]
            {
                "/App_Plugins/SeoToolkit/backoffice/NotFound/detail.controller.js",
                "/App_Plugins/SeoToolkit/backoffice/NotFound/pagenotfoundapi.resource.js"
            },
            BundleOptions = BundleOptions.None
        });
    }
}
