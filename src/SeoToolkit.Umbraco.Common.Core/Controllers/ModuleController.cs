using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Attributes;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models;
using System.Linq;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "seoToolkit")]
    public class ModuleController : SeoToolkitControllerBase
    {
        private readonly ModuleCollection _moduleCollection;

        public ModuleController(ModuleCollection moduleCollection)
        {
            _moduleCollection = moduleCollection;
        }

        [HttpGet("modules")]
        [ProducesResponseType(typeof(SeoToolkitModule[]), 200)]
        public IActionResult GetModules()
        {
            return Ok(_moduleCollection.GetAll().ToArray());
        }
    }
}
