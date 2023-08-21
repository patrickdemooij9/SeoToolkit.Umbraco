using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace SeoToolkit.Umbraco.Sitemap
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
                PackageName = "SeoToolkit.Umbraco.Sitemap",
                Version = "3.3.0",
                Scripts = new[]
                {
                    "/App_Plugins/SeoToolkit/Sitemap/Displays/DocumentType/sitemapSettings.controller.js"
                },
                BundleOptions = BundleOptions.None
            });
        }
    }
}
