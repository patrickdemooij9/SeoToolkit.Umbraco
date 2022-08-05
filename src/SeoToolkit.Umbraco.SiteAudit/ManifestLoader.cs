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
                Version = "2.0.0",
                Scripts = new[]
                {
                    "/App_Plugins/SeoToolkitSiteAudit/Interface/Dashboards/SiteAudit/siteAuditDashboard.controller.js",
                    "/App_Plugins/SeoToolkitSiteAudit/Interface/SiteAudit/create.controller.js",
                    "/App_Plugins/SeoToolkitSiteAudit/Interface/SiteAudit/detail.controller.js",
                    "/App_Plugins/SeoToolkitSiteAudit/Interface/SiteAudit/overview.controller.js",
                    "/App_Plugins/SeoToolkitSiteAudit/js/siteAuditHub.js",
                    "/App_Plugins/SeoToolkitSiteAudit/backoffice/SiteAudit/list.controller.js",
                    "/App_Plugins/SeoToolkitSiteAudit/backoffice/SiteAudit/detail.controller.js",
                    "/App_Plugins/SeoToolkitSiteAudit/backoffice/SiteAudit/create.controller.js",
                    "/App_Plugins/SeoToolkitSiteAudit/libs/chart.min.js"
                },
                Stylesheets = new[]
                {
                    "/App_Plugins/SeoToolkitSiteAudit/css/siteAudit.css"
                }
            });
        }
    }
}
