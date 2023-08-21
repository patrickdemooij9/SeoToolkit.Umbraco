using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace SeoToolkit.Umbraco.Redirects
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
                PackageName = "SeoToolkit.Umbraco.Redirects",
                Version = "3.3.0",
                Scripts = new[]
                {
                    "/App_Plugins/SeoToolkit/backoffice/Redirects/list.controller.js",
                    "/App_Plugins/SeoToolkit/Redirects/Dialogs/createRedirect.controller.js",
                    "/App_Plugins/SeoToolkit/Redirects/Dialogs/linkPicker.controller.js"
                },
                Stylesheets = new[]
                {
                    "/App_Plugins/SeoToolkit/Redirects/css/main.css"
                },
                BundleOptions = BundleOptions.None
            });
        }
    }
}
