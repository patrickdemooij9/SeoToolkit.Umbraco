using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Models.PostModel;
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
            _robotsTxtService.SetContent(model.Content ?? string.Empty);
            return Ok();
        }
    }
}
