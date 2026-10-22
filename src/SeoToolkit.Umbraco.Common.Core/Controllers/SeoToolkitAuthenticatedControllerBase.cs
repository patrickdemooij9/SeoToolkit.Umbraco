using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Constants;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Common.Filters;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiController]
    [BackOfficeRoute("seoToolkit")]
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    [MapToApi("seoToolkit")]
    // Use our own named JSON options, so neither site-wide serializer settings (e.g. PascalCase) nor
    // Umbraco's backoffice converters change the contract the backoffice client and value converters rely on
    [JsonOptionsName(JsonOptionsConstants.SeoToolkit)]
    public abstract class SeoToolkitAuthenticatedControllerBase : Controller
    {
    }
}
