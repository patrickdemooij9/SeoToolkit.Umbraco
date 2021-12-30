using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Controllers
{
    public class SitemapController : UmbracoController
    {
        public IActionResult Render()
        {
            return Ok();
        }
    }
}
