using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class SiteAuditPageCheckController : UmbracoAuthorizedApiController
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

        [HttpGet]
        public IActionResult GetPageChecks()
        {
            return new JsonResult(_siteCheckService.GetAll().Where(it => it.AllowedAsPageCheck).Select(it => new SiteAuditCheckViewModel { Id = it.Id, Name = it.Check.Name, Description = it.Check.Description }).ToArray());
        }

        [HttpPost]
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
            return new JsonResult(new SiteAuditDetailViewModel(result));
        }
    }
}
