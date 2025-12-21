using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkitSeoKeyValueSettings")]
    public class SeoKeyValueSettingsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly SeoKeyValueSettingCollection _seoKeyValueSettings;
        private readonly ISeoKeyValueRepository _seoKeyValueRepository;

        public SeoKeyValueSettingsController(SeoKeyValueSettingCollection seoKeyValueSettings, ISeoKeyValueRepository seoKeyValueRepository)
        {
            _seoKeyValueSettings = seoKeyValueSettings;
            _seoKeyValueRepository = seoKeyValueRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SeoKeyValueSettingViewModel[]), 200)]
        public IActionResult GetSettings(int? domainId)
        {
            _seoKeyValueRepository.Get
        }
    }
}
