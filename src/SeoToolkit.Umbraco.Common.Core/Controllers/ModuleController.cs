using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Attributes;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models;
using System.Linq;
using SeoToolkit.Umbraco.Common.Core.Enums;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    public class ModuleController : SeoToolkitAuthenticatedControllerBase
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

        [HttpGet("isEnabled")]
        [ProducesResponseType(typeof(bool), 200)]
        public IActionResult IsEnabled(string moduleAlias)
        {
            return Ok(_moduleCollection.GetAll().FirstOrDefault(it => it.Alias == moduleAlias)?.Status == SeoToolkitModuleStatus.Installed);
        }
    }
}
