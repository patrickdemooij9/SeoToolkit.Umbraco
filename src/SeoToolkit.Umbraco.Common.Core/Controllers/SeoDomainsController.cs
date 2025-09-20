using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkitDomains")]
    public class SeoDomainsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly ISeoDomainsService _domainsService;

        public SeoDomainsController(ISeoDomainsService domainsService)
        {
            _domainsService = domainsService;
        }

        [HttpPost("save")]
        public IActionResult Save(SeoDomainCollection collection)
        {
            _domainsService.Save(collection); //TODO: Return ID
            return Ok();
        }
    }
}
