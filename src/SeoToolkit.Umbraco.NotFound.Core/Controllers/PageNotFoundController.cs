using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace SeoToolkit.Umbraco.NotFound.Core.Controllers;

[PluginController("SeoToolkit")]
public class PageNotFoundController : UmbracoAuthorizedApiController
{
    private readonly IKeyValueService _keyValueService;

    public PageNotFoundController(IKeyValueService keyValueService)
    {
        _keyValueService = keyValueService;
    }

    [HttpPost]
    public void SetKeyValue([FromBody] string data)
    {
        _keyValueService.SetValue(NotFoundConstants.NotFoundKeyValueKey, data);
    }

    [HttpGet]
    public string GetValue()
    {
        return _keyValueService.GetValue(NotFoundConstants.NotFoundKeyValueKey) ?? "-1";
    }
}
