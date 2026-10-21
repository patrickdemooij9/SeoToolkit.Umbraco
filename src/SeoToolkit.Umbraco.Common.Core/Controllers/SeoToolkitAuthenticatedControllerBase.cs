using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiController]
    [BackOfficeRoute("seoToolkit")]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [MapToApi("seoToolkit")]
    // Use Umbraco's backoffice JSON options, so site-wide serializer settings (e.g. PascalCase) don't break the backoffice client
    [JsonOptionsName(UmbConstants.JsonOptionsNames.BackOffice)]
    public abstract class SeoToolkitAuthenticatedControllerBase : Controller
    {
    }
}
