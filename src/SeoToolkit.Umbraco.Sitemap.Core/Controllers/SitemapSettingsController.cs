using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.PostModels;
using SeoToolkit.Umbraco.Sitemap.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;

namespace SeoToolkit.Umbraco.Sitemap.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class SitemapSettingsController : UmbracoAuthorizedApiController
    {
        private readonly ISitemapService _sitemapService;

        public SitemapSettingsController(ISitemapService sitemapService)
        {
            _sitemapService = sitemapService;
        }

        [HttpGet]
        public IActionResult GetPageTypeSettings(int contentTypeId)
        {
            var settings = _sitemapService.GetPageTypeSettings(contentTypeId) ?? new SitemapPageSettings();
            return new JsonResult(new SitemapPageTypeSettingsViewModel
            {
                HideFromSitemap = settings.HideFromSitemap,
                ChangeFrequency = settings.ChangeFrequency,
                Priority = settings.Priority
            });
        }

        [HttpPost]
        public IActionResult SetPageTypeSettings(SitemapPageTypeSettingsPostModel model)
        {
            _sitemapService.SetPageTypeSettings(new SitemapPageSettings
            {
                ContentTypeId = model.ContentTypeId,
                HideFromSitemap = model.HideFromSitemap,
                ChangeFrequency = model.ChangeFrequency,
                Priority = model.Priority
            });
            return Ok();
        }
    }
}
