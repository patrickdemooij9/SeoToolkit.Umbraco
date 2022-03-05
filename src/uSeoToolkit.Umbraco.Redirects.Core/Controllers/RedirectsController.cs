using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace uSeoToolkit.Umbraco.Redirects.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class RedirectsController : UmbracoAuthorizedApiController
    {
        public RedirectsController()
        {
            
        }

        public IActionResult GetAll()
        {
            return Ok();
        }
    }
}
