using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace SeoToolkit.Umbraco.SiteAudit
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
                PackageName = "SeoToolkit.Umbraco.SiteAudit",
                Version = "2.4.0",
                Scripts = new[]
                {
                    "/App_Plugins/SeoToolkit/SiteAudit/Interface/Dashboards/SiteAudit/siteAuditDashboard.controller.js",
                    "/App_Plugins/SeoToolkit/SiteAudit/Interface/SiteAudit/create.controller.js",
                    "/App_Plugins/SeoToolkit/SiteAudit/Interface/SiteAudit/detail.controller.js",
                    "/App_Plugins/SeoToolkit/SiteAudit/Interface/SiteAudit/overview.controller.js",
                    "/App_Plugins/SeoToolkit/SiteAudit/js/siteAuditHub.js",
                    "/App_Plugins/SeoToolkit/backoffice/SiteAudit/list.controller.js",
                    "/App_Plugins/SeoToolkit/backoffice/SiteAudit/detail.controller.js",
                    "/App_Plugins/SeoToolkit/backoffice/SiteAudit/create.controller.js"
                },
                Stylesheets = new[]
                {
                    "/App_Plugins/SeoToolkit/SiteAudit/css/siteAudit.css"
                },
                BundleOptions = BundleOptions.None
            });
        }
    }
}
