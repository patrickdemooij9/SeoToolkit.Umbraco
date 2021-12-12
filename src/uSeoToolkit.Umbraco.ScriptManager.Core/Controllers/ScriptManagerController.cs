using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.ScriptManager.Core.Collections;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels;
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

        public IActionResult Get(int id)
        {
            var script = _scriptManagerService.Get(id);
            if (script is null)
                return NotFound();

            return new JsonResult(new ScriptDetailViewModel(script));
        }

        public IActionResult Save(CreateScriptPostModel postModel)
        {
            var definition = _scriptDefinitionCollection.FirstOrDefault(it => it.Alias == postModel.DefinitionAlias);
            if (definition is null)
                return NotFound();
            var script = new Script
            {
                Id = postModel.Id,
                Name = postModel.Name,
                Definition = definition,
                Config = postModel.Fields.ToDictionary(it => it.Key, it => it.Value)
            };
            _scriptManagerService.Save(script);
            return Ok();
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

        [HttpPost]
        public IActionResult Delete(DeleteScriptPostModel postModel)
        {
            _scriptManagerService.Delete(postModel.Ids);
            return GetAllScripts();
        }
    }
}
