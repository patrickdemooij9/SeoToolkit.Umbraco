using System;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Sitemap.Core.Controllers;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Components
{
    public class SitemapRegistrationComponent : IAreaRoutes
    {
        private readonly IRuntimeState _runtimeState;

        public SitemapRegistrationComponent(IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
        }
        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return;
            
            endpoints.MapUmbracoRoute<SitemapController>("sitemap.xml", "sitemap.xml", string.Empty, "Render", false);
        }
    }
}
