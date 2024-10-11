using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.PostModel;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.ResponseModel;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "seoToolkit")]
    public class RobotsTxtController : SeoToolkitControllerBase
    {
        private readonly IRobotsTxtService _robotsTxtService;

        public RobotsTxtController(IRobotsTxtService robotsTxtService)
        {
            _robotsTxtService = robotsTxtService;
        }

        [HttpGet("robotsTxt")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Get()
        {
            return Ok(_robotsTxtService.GetContent());
        }

        [HttpPost("robotsTxt")]
        [ProducesResponseType(typeof(RobotsTxtSaveResponseModel), 200)]
        public IActionResult Save(RobotsTxtSavePostModel model)
        {
            var content = model.Content ?? string.Empty;
            
            // After saving the content with any validation errors, the user gets the option to save without validation.
            if (!model.SkipValidation)
            {
                var validationErrors = _robotsTxtService.Validate(content).ToArray();
                if (validationErrors.Length > 0)
                {
                    return Ok(new RobotsTxtSaveResponseModel
                    {
                        Content = model.Content,
                        Errors = validationErrors.Select(it => new RobotsTxtValidationViewModel(it)).ToArray()
                    });
                }
            }

            _robotsTxtService.SetContent(content);
            return Ok(new RobotsTxtSaveResponseModel
            {
                Content = content
            });
        }
    }
}
