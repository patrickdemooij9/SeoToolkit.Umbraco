using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.PostModels;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using Umbraco.Cms.Api.Management.Controllers;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class SeoSettingsController : ManagementApiControllerBase
    {
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly DisplayCollection _displayCollection;

        public SeoSettingsController(ISeoSettingsService seoSettingsService, DisplayCollection displayCollection)
        {
            _seoSettingsService = seoSettingsService;
            _displayCollection = displayCollection;
        }

        [HttpGet("seoSettings")]
        public IActionResult Get(int contentTypeId)
        {
            return new JsonResult(new SeoSettingsViewModel
            {
                IsEnabled = _seoSettingsService.IsEnabled(contentTypeId),
                SupressContentAppSavingNotification = _seoSettingsService.SupressContentAppSavingNotification(), //TODO from settings
                Displays = _displayCollection.Select(it => it.Get(contentTypeId)).WhereNotNull().ToArray()
            });
        }

        [HttpPost("seoSettings")]
        public IActionResult Set(SeoSettingsPostModel postModel)
        {
            _seoSettingsService.ToggleSeoSettings(postModel.ContentTypeId, postModel.Enabled);
            return Ok();
        }
    }
}
