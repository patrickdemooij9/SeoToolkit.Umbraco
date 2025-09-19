using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiController]
    [Route("/api/seoToolkit/{controller}")]
    [MapToApi("seoToolkit")]
    [ApiExplorerSettings(GroupName = "SeoToolkit Public Api")]
    public abstract class SeoToolkitPublicControllerBase : Controller
    {
    }
}
