using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkitDomains")]
    public class SeoDomainsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly ISeoDomainsService _seoDomainsService;
        private readonly IDomainService _domainService;
        private readonly SeoTreeSectionCollection _seoTreeSections;

        public SeoDomainsController(ISeoDomainsService seoDomainsService, IDomainService domainService, SeoTreeSectionCollection seoTreeSections)
        {
            _seoDomainsService = seoDomainsService;
            _domainService = domainService;
            _seoTreeSections = seoTreeSections;
        }

        [HttpGet("get")]
        [ProducesResponseType(typeof(SeoDomainCollection), 200)]
        public IActionResult Get(int domainId)
        {
            return Ok(_seoDomainsService.GetAll().FirstOrDefault(it => it.Id == domainId));
        }

        [HttpPost("save")]
        public IActionResult Save(SeoDomainCollection collection)
        {
            _seoDomainsService.Save(collection); //TODO: Return ID
            return Ok();
        }

        [HttpGet("config")]
        [ProducesResponseType(typeof(SeoDomainConfigViewModel), 200)]
        public async Task<IActionResult> GetConfig()
        {
            var config = new SeoDomainConfigViewModel
            {
                Domains = [.. (await _domainService.GetAllAsync(false))],
                ModuleSettings = [.. _seoTreeSections.Where(it => it.CanBeDomainSpecific).Select(it => new SeoDomainModuleSettingViewModel
                {
                    Id = it.Id,
                    Name = it.Name
                })]
            };
            return Ok(config);
        }
    }
}
