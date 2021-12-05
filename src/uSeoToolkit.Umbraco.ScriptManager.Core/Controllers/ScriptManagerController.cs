using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.ScriptManager.Core.Collections;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class ScriptManagerController : UmbracoAuthorizedApiController
    {
        private readonly ScriptDefinitionCollection _scriptDefinitionCollection;
        private readonly IScriptManagerService _scriptManagerService;

        public ScriptManagerController(ScriptDefinitionCollection scriptDefinitionCollection, IScriptManagerService scriptManagerService)
        {
            _scriptDefinitionCollection = scriptDefinitionCollection;
            _scriptManagerService = scriptManagerService;
        }

        [HttpGet]
        public IActionResult GetAllScripts()
        {
            return new JsonResult(_scriptManagerService.GetAll().Select(it => new ScriptListViewModel(it)));

        }

        [HttpGet]
        public IActionResult GetAllDefinitions()
        {
            return new JsonResult(_scriptDefinitionCollection.Select(it => new ScriptDefinitionViewModel(it)));
        }
    }
}
