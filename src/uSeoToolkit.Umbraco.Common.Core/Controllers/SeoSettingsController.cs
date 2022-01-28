using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Collections;
using uSeoToolkit.Umbraco.Common.Core.Models.PostModels;
using uSeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using uSeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;

namespace uSeoToolkit.Umbraco.Common.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class SeoSettingsController : UmbracoAuthorizedApiController
    {
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly DisplayCollection _displayCollection;

        public SeoSettingsController(ISeoSettingsService seoSettingsService, DisplayCollection displayCollection)
        {
            _seoSettingsService = seoSettingsService;
            _displayCollection = displayCollection;
        }

        public IActionResult Get(int contentTypeId)
        {
            return new JsonResult(new SeoSettingsViewModel
            {
                IsEnabled = _seoSettingsService.IsEnabled(contentTypeId),
                Displays = _displayCollection.Select(it => it.Get(contentTypeId)).WhereNotNull().ToArray()
            });
        }

        public IActionResult Set(SeoSettingsPostModel postModel)
        {
            _seoSettingsService.ToggleSeoSettings(postModel.ContentTypeId, postModel.Enabled);
            return Ok();
        }
    }
}
