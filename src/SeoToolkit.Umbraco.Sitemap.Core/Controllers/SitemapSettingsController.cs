using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.PostModels;
using SeoToolkit.Umbraco.Sitemap.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using System;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Sitemap.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit Sitemap")]
    [BackOfficeRoute("seoToolkitSitemap")]
    public class SitemapSettingsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly ISitemapService _sitemapService;
        private readonly IContentTypeService _contentTypeService;

        public SitemapSettingsController(ISitemapService sitemapService, IContentTypeService contentTypeService)
        {
            _sitemapService = sitemapService;
            _contentTypeService = contentTypeService;
        }

        [HttpGet("sitemapSettings")]
        [ProducesResponseType(typeof(SitemapPageTypeSettingsViewModel), 200)]
        public IActionResult GetPageTypeSettings(Guid contentTypeGuid)
        {
            var settings = new SitemapPageSettings();
            var contentType = _contentTypeService.Get(contentTypeGuid);
            if (contentType != null)
            {
                settings = _sitemapService.GetPageTypeSettings(contentType.Id) ?? new SitemapPageSettings();
            }

            return new JsonResult(new SitemapPageTypeSettingsViewModel
            {
                HideFromSitemap = settings.HideFromSitemap,
                ChangeFrequency = settings.ChangeFrequency,
                Priority = settings.Priority
            });
        }

        [HttpPost("sitemapSettings")]
        public IActionResult SetPageTypeSettings(SitemapPageTypeSettingsPostModel model)
        {
            var contentType = _contentTypeService.Get(model.ContentTypeGuid);
            if (contentType is null) return NotFound();

            _sitemapService.SetPageTypeSettings(new SitemapPageSettings
            {
                ContentTypeId = contentType.Id,
                HideFromSitemap = model.HideFromSitemap,
                ChangeFrequency = model.ChangeFrequency,
                Priority = model.Priority
            });
            return Ok();
        }
    }
}
