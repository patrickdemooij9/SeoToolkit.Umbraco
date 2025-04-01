using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.NotFound.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "seoToolkitNotFound")]
    [BackOfficeRoute("seoToolkitNotFound")]
    public class PageNotFoundController : SeoToolkitControllerBase
    {
        private readonly IKeyValueService _keyValueService;

        public PageNotFoundController(IKeyValueService keyValueService)
        {
            _keyValueService = keyValueService;
        }

        [HttpPost("notFound")]
        public void SetKeyValue(string data)
        {
            _keyValueService.SetValue(NotFoundConstants.NotFoundKeyValueKey, data);
        }

        [HttpGet("notFound")]
        [ProducesResponseType(typeof(string), 200)]
        public string GetValue()
        {
            return _keyValueService.GetValue(NotFoundConstants.NotFoundKeyValueKey) ?? "-1";
        }
    }
}
