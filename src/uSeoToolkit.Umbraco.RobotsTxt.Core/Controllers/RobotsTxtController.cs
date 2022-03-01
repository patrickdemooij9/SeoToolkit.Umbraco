using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Collections;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Models.PostModel;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Models.ViewModels;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Services;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class RobotsTxtController : UmbracoAuthorizedApiController
    {
        private readonly IRobotsTxtService _robotsTxtService;
        private readonly RobotsTxtShortcutCollection _shortcutCollection;

        public RobotsTxtController(IRobotsTxtService robotsTxtService, RobotsTxtShortcutCollection shortcutCollection)
        {
            _robotsTxtService = robotsTxtService;
            _shortcutCollection = shortcutCollection;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(_robotsTxtService.GetContent());
        }

        [HttpGet]
        public IActionResult GetShortcuts()
        {
            return Ok(_shortcutCollection.Select(it => new RobotsTxtShortcutViewModel(it)));
        }

        [HttpPost]
        public IActionResult HandleShortcut(RobotsTxtHandleShortcutPostModel model)
        {
            var shortcut = _shortcutCollection.FirstOrDefault(it => it.Alias == model.Alias);
            if (shortcut is null) return BadRequest();

            var result = shortcut.Execute(new Dictionary<string, string>());
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Save(RobotsTxtSavePostModel model)
        {
            var content = model.Content ?? string.Empty;
            var validationErrors = _robotsTxtService.Validate(content).ToArray();
            if (validationErrors.Length > 0)
            {
                return BadRequest(validationErrors.Select(it => new RobotsTxtValidationViewModel(it)));
            }

            _robotsTxtService.SetContent(model.Content);
            return Get();
        }
    }
}
