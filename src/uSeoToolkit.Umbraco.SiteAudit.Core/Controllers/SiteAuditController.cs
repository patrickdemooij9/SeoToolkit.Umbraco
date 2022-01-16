using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.SiteAudit.Core.Collections;
using uSeoToolkit.Umbraco.SiteAudit.Core.Common.Scheduler;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;
using uSeoToolkit.Umbraco.SiteAudit.Core.Hubs;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;
using uSeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    [PluginController("uSeoToolkit")]
    public class SiteAuditController : UmbracoAuthorizedApiController
    {
        private readonly SiteAuditService _siteAuditService;
        private readonly ISiteCheckService _siteCheckService;
        private readonly SiteAuditHubClientService _siteAuditHubClient;
        private readonly ISettingsService<SiteAuditConfigModel> _settingsService;
        private readonly ILogger<SiteAuditController> _logger;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteAuditScheduler _siteAuditScheduler;

        public SiteAuditController(SiteAuditService siteAuditService,
            ISiteCheckService siteCheckService,
            SiteAuditHubClientService siteAuditHubClient,
            ISettingsService<SiteAuditConfigModel> settingsService,
            ILogger<SiteAuditController> logger,
            IUmbracoContextFactory umbracoContextFactory,
            ISiteAuditScheduler siteAuditScheduler)
        {
            _siteAuditService = siteAuditService;
            _siteCheckService = siteCheckService;
            _siteAuditHubClient = siteAuditHubClient;
            _settingsService = settingsService;
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
            _siteAuditScheduler = siteAuditScheduler;
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            var model = _siteAuditService.Get(id);
            if (model is null)
                return NotFound();

            return new JsonResult(new SiteAuditDetailViewModel(model));
        }

        [HttpPost]
        public IActionResult Delete(DeleteAuditsPostModel postModel)
        {
            foreach (var id in postModel.Ids)
            {
                _siteAuditService.Delete(id);
            }

            return GetAll();
        }

        //SignalR stuff
        [HttpGet]
        public IActionResult Connect(int auditId, string clientId)
        {
            _siteAuditHubClient.AssignClient(clientId, auditId);
            return Get(auditId);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var test = _settingsService.GetSettings();
            return new JsonResult(_siteAuditService.GetAll().Select(it => new SiteAuditOverviewViewModel
            {
                Id = it.Id,
                Name = it.Name,
                CreatedDate = it.CreatedDate.ToShortDateString(),
                Status = it.Status.ToString() //TODO: Replace with string from translation file
            }));
        }

        [HttpGet]
        public IActionResult GetAllChecks()
        {
            return new JsonResult(_siteCheckService.GetAll().Select(it => new SiteAuditCheckViewModel { Id = it.Id, Name = it.Check.Name, Description = it.Check.Description }));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAudit([FromBody] CreateAuditPostModel postModel)
        {
            //TODO: Move to mapper
            SiteAuditDto model;
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                model = new SiteAuditDto
                {
                    Name = postModel.Name,
                    CreatedDate = DateTime.UtcNow,
                    Status = postModel.StartAudit ? SiteAuditStatus.Scheduled : SiteAuditStatus.Created,
                    StartingUrl = new Uri(ctx.UmbracoContext.Content.GetById(postModel.SelectedNodeId).Url(mode: UrlMode.Absolute)),
                    SiteChecks = _siteCheckService.GetAll().Where(it => postModel.Checks.Contains(it.Id)).ToList(),
                    MaxPagesToCrawl = postModel.MaxPagesToCrawl == 0 ? (int?)null : postModel.MaxPagesToCrawl,
                    DelayBetweenRequests = postModel.DelayBetweenRequests * 1000
                };
            }

            model = _siteAuditService.Save(model);
            if (postModel.StartAudit)
            {
                _siteAuditScheduler.AddSiteAudit(model.Id);
            }
            return Ok(model.Id);
        }

        [HttpPost]
        public IActionResult StopAudit(StopAuditPostModel model)
        {
            _siteAuditService.StopSiteAudit(model.Id);
            return Ok();
        }
    }
}
