using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.PostModels;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using Umbraco.Cms.Api.Management.Controllers;
using System;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "seoToolkit")]
    [BackOfficeRoute("seoToolkitSettings")]
    public class SeoSettingsController : SeoToolkitControllerBase
    {
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly DisplayCollection _displayCollection;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SeoSettingsController(ISeoSettingsService seoSettingsService, DisplayCollection displayCollection,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _seoSettingsService = seoSettingsService;
            _displayCollection = displayCollection;
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet("seoSettings")]
        [ProducesResponseType(typeof(SeoSettingsViewModel), 200)]
        public IActionResult Get(Guid contentTypeId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var contentType = ctx.UmbracoContext.Content?.GetContentType(contentTypeId);
            if (contentType is null) return NotFound();

            return new JsonResult(new SeoSettingsViewModel
            {
                IsEnabled = _seoSettingsService.IsEnabled(contentType.Id),
                SupressContentAppSavingNotification = _seoSettingsService.SupressContentAppSavingNotification(),
                //Displays = _displayCollection.Select(it => it.Get(contentTypeId)).WhereNotNull().ToArray()
            });
        }

        [HttpPost("seoSettings")]
        public IActionResult Set(SeoSettingsPostModel postModel)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var contentType = ctx.UmbracoContext.Content?.GetContentType(postModel.ContentTypeId);
            if (contentType is null) return NotFound();

            _seoSettingsService.ToggleSeoSettings(contentType.Id, postModel.Enabled);
            return Ok();
        }
    }
}
