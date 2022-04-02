using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Hubs
{
    public class SiteAuditHubRoutes : IAreaRoutes
    {
        private readonly IRuntimeState _runtimeState;
        private readonly string _umbracoPathSegment;

        public SiteAuditHubRoutes(
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            _umbracoPathSegment = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
        }

        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            switch (_runtimeState.Level)
            {
                case RuntimeLevel.Run:
                    endpoints.MapHub<SiteAuditHub>(GetHubRoute());
                    break;
            }

        }

        public string GetHubRoute()
        {
            return $"/{_umbracoPathSegment}/{nameof(SiteAuditHub)}";
        }
    }
}
