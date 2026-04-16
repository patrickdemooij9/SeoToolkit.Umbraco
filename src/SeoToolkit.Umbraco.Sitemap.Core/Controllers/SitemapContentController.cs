using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Models.PostModels;
using SeoToolkit.Umbraco.Sitemap.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using System;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Sitemap.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit Sitemap")]
    [BackOfficeRoute("seoToolkitSitemapContent")]
    public class SitemapContentController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly ISitemapService _sitemapService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SitemapContentController(ISitemapService sitemapService, IUmbracoContextFactory umbracoContextFactory)
        {
            _sitemapService = sitemapService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet("contentSettings")]
        [ProducesResponseType(typeof(SitemapContentSettingsViewModel), 200)]
        public IActionResult GetContentSettings(Guid nodeKey)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content?.GetById(true, nodeKey);
            if (content is null) return NotFound();

            var contentOverride = _sitemapService.GetContentSettings(nodeKey);
            var docTypeSettings = _sitemapService.GetPageTypeSettings(content.ContentType.Key);

            bool effectiveHide = contentOverride?.HideFromSitemap ?? docTypeSettings?.HideFromSitemap ?? false;
            string effectiveFrequency = contentOverride?.ChangeFrequency ?? docTypeSettings?.ChangeFrequency;
            double? effectivePriority = contentOverride?.Priority ?? docTypeSettings?.Priority;

            return Ok(new SitemapContentSettingsViewModel
            {
                HideFromSitemap = contentOverride?.HideFromSitemap,
                ChangeFrequency = contentOverride?.ChangeFrequency,
                Priority = contentOverride?.Priority,
                EffectiveHideFromSitemap = effectiveHide,
                EffectiveChangeFrequency = effectiveFrequency,
                EffectivePriority = effectivePriority
            });
        }

        [HttpPost("contentSettings")]
        public IActionResult SetContentSettings(SitemapContentSettingsPostModel model)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var content = ctx.UmbracoContext.Content?.GetById(true, model.NodeKey);
            if (content is null) return NotFound();

            _sitemapService.SetContentSettings(new SitemapContentSettings
            {
                NodeKey = model.NodeKey,
                HideFromSitemap = model.HideFromSitemap,
                ChangeFrequency = model.ChangeFrequency,
                Priority = model.Priority
            });

            return Ok();
        }
    }
}
