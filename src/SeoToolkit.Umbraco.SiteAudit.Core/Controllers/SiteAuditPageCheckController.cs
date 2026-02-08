using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit SiteAudit")]
    [BackOfficeRoute("seoToolkitSiteAudit")]
    public class SiteAuditPageCheckController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly SiteAuditService _siteAuditService;
        private readonly ISiteCheckService _siteCheckService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SiteAuditPageCheckController(SiteAuditService siteAuditService, ISiteCheckService siteCheckService, IUmbracoContextFactory umbracoContextFactory)
        {
            _siteAuditService = siteAuditService;
            _siteCheckService = siteCheckService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        [HttpGet("pageChecks")]
        [ProducesResponseType(typeof(SiteAuditCheckViewModel[]), 200)]
        public IActionResult GetPageChecks()
        {
            return Ok(_siteCheckService.GetAll().Where(it => it.AllowedAsPageCheck).Select(it => new SiteAuditCheckViewModel { Id = it.Id, Name = it.Check.Name, Description = it.Check.Description }).ToArray());
        }

        [HttpPost("run")]
        [ProducesResponseType(typeof(SiteAuditDetailViewModel), 200)]
        public async Task<IActionResult> RunPageChecks(RunPageCheckPostModel postModel)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var model = new SiteAuditDto
            {
                Name = string.Empty,
                CreatedDate = DateTime.UtcNow,
                StartingUrl = new Uri(ctx.UmbracoContext.Content.GetById(postModel.ContentId).Url(mode: UrlMode.Absolute)),
                SiteChecks = _siteCheckService.GetAll().Where(it => it.AllowedAsPageCheck).ToList(),
                MaxPagesToCrawl = 1,
                DelayBetweenRequests = 1000,
                Persistent = false
            };

            var result = await _siteAuditService.StartSiteAudit(model);
            return Ok(new SiteAuditDetailViewModel(result));
        }
    }
}