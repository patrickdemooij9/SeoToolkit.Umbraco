using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace SeoToolkit.Umbraco.Common
{
    internal class ManifestLoader : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<ManifestFilter>();
        }
    }

    internal class ManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "SeoToolkit.Umbraco.Common",
                Version = "3.5.0",
                Scripts = new[]
                {
                    "/App_Plugins/SeoToolkit/Dashboards/welcomeDashboard.controller.js",
                    "/App_Plugins/SeoToolkit/ContentApps/DocumentType/seoSettings.controller.js",
                    "/App_Plugins/SeoToolkit/ContentApps/Content/seoContent.controller.js"
                },
                Stylesheets = new[]
                {
                    "/App_Plugins/SeoToolkit/css/main.css"
                },
                BundleOptions = BundleOptions.None
            });
        }
    }
}
