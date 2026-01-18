using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IActionResult GetSettings(Guid? domainId)
        {
            var values = _seoKeyValueRepository.Get(domainId);
            var rootValues = values;
            if (domainId.HasValue)
            {
                rootValues = _seoKeyValueRepository.Get(null);
            }
            return Ok(_seoKeyValueSettings.Select(it => new SeoKeyValueSettingViewModel
            {
                Key = it.Key,
                Title = it.Title,
                Description = it.Description,
                PropertyAlias = it.PropertyAlias,
                Value = values.TryGetValue(it.Key, out string? value) ? value : null,
                HasRootValue = rootValues.ContainsKey(it.Key)
            }));
        }

        [HttpPost("save")]
        public IActionResult SaveSettings(Dictionary<string, string> values, Guid? domainId)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value.Value))
                {
                    _seoKeyValueRepository.Delete(value.Key, domainId);
                    continue;
                }

                _seoKeyValueRepository.Set(value.Key, value.Value, domainId);
            }
            return Ok();
        }
    }
}
