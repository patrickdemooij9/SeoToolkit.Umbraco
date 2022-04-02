using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using SeoToolkit.Umbraco.Common.Core.Collections;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class ModuleController : UmbracoAuthorizedApiController
    {
        private readonly ModuleCollection _moduleCollection;

        public ModuleController(ModuleCollection moduleCollection)
        {
            _moduleCollection = moduleCollection;
        }

        [HttpGet]
        public IActionResult GetModules()
        {
            return new JsonResult(_moduleCollection.GetAll());
        }
    }
}
