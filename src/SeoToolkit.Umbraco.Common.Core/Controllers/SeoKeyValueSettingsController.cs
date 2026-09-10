using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkitSeoKeyValueSettings")]
    public class SeoKeyValueSettingsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly SeoKeyValueSettingCollection _seoKeyValueSettings;
        private readonly ISeoKeyValueRepository _seoKeyValueRepository;
        private readonly IEventAggregator _eventAggregator;

        public SeoKeyValueSettingsController(SeoKeyValueSettingCollection seoKeyValueSettings, ISeoKeyValueRepository seoKeyValueRepository, IEventAggregator eventAggregator)
        {
            _seoKeyValueSettings = seoKeyValueSettings;
            _seoKeyValueRepository = seoKeyValueRepository;
            _eventAggregator = eventAggregator;
        }

        [HttpGet("value")]
        [ProducesResponseType(typeof(SeoKeyValueViewModel), 200)]
        public IActionResult GetValue(string key)
        {
            var values = _seoKeyValueRepository.Get(null);
            var value = string.Empty;
            if (values.TryGetValue(key, out var foundValue))
            {
                value = foundValue;
            }
            return Ok(new SeoKeyValueViewModel
            {
                Key = key,
                Value = value
            });
        }

        [HttpGet]
        [ProducesResponseType(typeof(SeoKeyValueSettingViewModel[]), 200)]
        public IActionResult GetSettings(Guid? domainId)
        {
            var values = _seoKeyValueRepository.Get(domainId);
            return Ok(_seoKeyValueSettings.Select(it => new SeoKeyValueSettingViewModel
            {
                Key = it.Key,
                Title = it.Title,
                Description = it.Description,
                PropertyAlias = it.PropertyAlias,
                Value = values.TryGetValue(it.Key, out string? value) ? Convert.ChangeType(value, it.EditorType) : null,
                IsRoot = !domainId.HasValue
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

            _eventAggregator.Publish(new SeoKeyValueSavedNotification(domainId));
            return Ok();
        }
    }
}
