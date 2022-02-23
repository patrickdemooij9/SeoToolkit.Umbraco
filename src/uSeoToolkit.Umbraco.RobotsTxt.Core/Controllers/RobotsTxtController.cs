using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
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

        public RobotsTxtController(IRobotsTxtService robotsTxtService)
        {
            _robotsTxtService = robotsTxtService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(_robotsTxtService.GetContent());
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
