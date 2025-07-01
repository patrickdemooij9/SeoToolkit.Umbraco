using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.PostModels;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using Umbraco.Cms.Api.Management.Controllers;
using System;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkitSettings")]
    public class SeoSettingsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentTypeService _contentTypeService;

        public SeoSettingsController(ISeoSettingsService seoSettingsService,
            IUmbracoContextFactory umbracoContextFactory,
            IContentTypeService contentTypeService)
        {
            _seoSettingsService = seoSettingsService;
            _umbracoContextFactory = umbracoContextFactory;
            _contentTypeService = contentTypeService;
        }

        [HttpGet("seoSettings")]
        [ProducesResponseType(typeof(SeoSettingsViewModel), 200)]
        public IActionResult Get(Guid contentTypeId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var contentType = _contentTypeService.Get(contentTypeId);
            var isEnabled = false;
            if (contentType is not null)
            {
                //TODO: Refactor this to use the content type from ContentTypeService
                isEnabled = _seoSettingsService.IsEnabled(contentType);
            }

            return new JsonResult(new SeoSettingsViewModel
            {
                IsEnabled = isEnabled,
                SupressContentAppSavingNotification = _seoSettingsService.SupressContentAppSavingNotification()
            });
        }

        [HttpPost("seoSettings")]
        public IActionResult Set(SeoSettingsPostModel postModel)
        {
            var contentType = _contentTypeService.Get(postModel.ContentTypeId);
            if (contentType is null) return NotFound();

            _seoSettingsService.ToggleSeoSettings(contentType.Id, postModel.Enabled);
            return Ok();
        }
    }
}
