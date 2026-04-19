using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Routing;
using System;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit ScriptManager")]
    [BackOfficeRoute("seoToolkitScriptManager")]
    public class ScriptManagerController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly ScriptDefinitionCollection _scriptDefinitionCollection;
        private readonly IScriptManagerService _scriptManagerService;

        public ScriptManagerController(ScriptDefinitionCollection scriptDefinitionCollection, IScriptManagerService scriptManagerService)
        {
            _scriptDefinitionCollection = scriptDefinitionCollection;
            _scriptManagerService = scriptManagerService;
        }

        [HttpGet("script")]
        [ProducesResponseType(typeof(ScriptDetailViewModel), 200)]
        public IActionResult Get(Guid id)
        {
            var script = _scriptManagerService.Get(id);
            if (script is null)
                return NotFound();

            return Ok(new ScriptDetailViewModel(script));
        }

        [HttpPost("script")]
        [ProducesResponseType(typeof(ScriptDetailViewModel), 200)]
        public IActionResult Save(CreateScriptPostModel postModel)
        {
            var definition = _scriptDefinitionCollection.FirstOrDefault(it => it.Alias == postModel.DefinitionAlias);
            if (definition is null)
                return NotFound();
            var script = new Script
            {
                Id = postModel.Id,
                Key = postModel.Key,
                Name = postModel.Name,
                Definition = definition,
                Config = postModel.Fields.ToDictionary(it => it.Key, it => it.Value),
                DomainId = postModel.DomainId,
                SortOrder = postModel.SortOrder ?? 0
            };
            script = _scriptManagerService.Save(script);
            return Ok(new ScriptDetailViewModel(script));
        }

        [HttpGet("scripts")]
        [ProducesResponseType(typeof(ScriptListViewModel[]), 200)]
        public IActionResult GetAllScripts(Guid? domainId)
        {
            return Ok(_scriptManagerService.GetAll(domainId).Select(it => new ScriptListViewModel(it)));

        }

        [HttpGet("definitions")]
        [ProducesResponseType(typeof(ScriptDefinitionViewModel[]), 200)]
        public IActionResult GetAllDefinitions()
        {
            return Ok(_scriptDefinitionCollection.GetAll().Select(it => new ScriptDefinitionViewModel(it)));
        }

        [HttpDelete("script")]
        [ProducesResponseType(typeof(ScriptListViewModel[]), 200)]
        public IActionResult Delete(DeleteScriptPostModel postModel)
        {
            var domainId = _scriptManagerService.Get(postModel.Ids.FirstOrDefault())?.DomainId;

            _scriptManagerService.Delete(postModel.Ids);
            return GetAllScripts(domainId);
        }

        [HttpPost("sortScripts")]
        [ProducesResponseType(200)]
        public IActionResult SortScripts(SortScriptsPostModel postModel)
        {
            _scriptManagerService.Sort(postModel.Keys);
            return Ok();
        }
    }
}
