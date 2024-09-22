﻿using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace SeoToolkit.Umbraco.ScriptManager
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
                PackageName = "SeoToolkit.Umbraco.ScriptManager",
                Version = "4.0.0",
                Scripts = new[]
                {
                    "/App_Plugins/SeoToolkit/backoffice/ScriptManager/list.controller.js",
                    "/App_Plugins/SeoToolkit/backoffice/ScriptManager/edit.controller.js"
                },
                BundleOptions = BundleOptions.None
            });
        }
    }
}
