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

        [HttpGet("getPredefined")]
        [ProducesResponseType(typeof(SeoDomainCollection), 200)]
        public IActionResult GetPredefined(int umbracoDomainId)
        {
            var umbracoDomain = _domainService.GetById(umbracoDomainId);
            if (umbracoDomain is null) return NotFound();

            return Ok(new SeoDomainCollection
            {
                Name = umbracoDomain.DomainName.Replace("https://", ""),
                DomainIds = [umbracoDomainId]
            });
        }

        [HttpPost("save")]
        [ProducesResponseType(typeof(int), 200)]
        public IActionResult Save(SeoDomainCollection collection)
        {
            collection.DomainIds ??= [];
            collection.Settings ??= [];
            return Ok(_seoDomainsService.Save(collection));
        }

        [HttpDelete("delete")]
        [ProducesResponseType(200)]
        public IActionResult Delete(int domainId)
        {
            _seoDomainsService.Delete(domainId);
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
