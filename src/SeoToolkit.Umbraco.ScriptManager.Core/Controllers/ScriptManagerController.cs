using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Controllers;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "seoToolkit")]
    public class ScriptManagerController : SeoToolkitControllerBase
    {
        private readonly ScriptDefinitionCollection _scriptDefinitionCollection;
        private readonly IScriptManagerService _scriptManagerService;

        public ScriptManagerController(ScriptDefinitionCollection scriptDefinitionCollection, IScriptManagerService scriptManagerService)
        {
            _scriptDefinitionCollection = scriptDefinitionCollection;
            _scriptManagerService = scriptManagerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ScriptDetailViewModel), 200)]
        public IActionResult Get(int id)
        {
            var script = _scriptManagerService.Get(id);
            if (script is null)
                return NotFound();

            return Ok(new ScriptDetailViewModel(script));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ScriptDetailViewModel), 200)]
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
                Config = postModel.Fields.ToDictionary(it => it.Key, it => (object) it.Value)
            };
            script = _scriptManagerService.Save(script);
            return Ok(new ScriptDetailViewModel(script));
        }

        [HttpGet("All")]
        [ProducesResponseType(typeof(ScriptListViewModel[]), 200)]
        public IActionResult GetAllScripts()
        {
            return Ok(_scriptManagerService.GetAll().Select(it => new ScriptListViewModel(it)));

        }

        [HttpGet("Definitions")]
        [ProducesResponseType(typeof(ScriptDefinitionViewModel[]), 200)]
        public IActionResult GetAllDefinitions()
        {
            return Ok(_scriptDefinitionCollection.GetAll().Select(it => new ScriptDefinitionViewModel(it)));
        }

        [HttpDelete]
        [ProducesResponseType(typeof(ScriptListViewModel[]), 200)]
        public IActionResult Delete(DeleteScriptPostModel postModel)
        {
            _scriptManagerService.Delete(postModel.Ids);
            return GetAllScripts();
        }
    }
}
