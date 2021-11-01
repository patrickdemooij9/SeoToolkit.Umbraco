using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.SiteAudit.Core.Collections;
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

        public SiteAuditController(SiteAuditService siteAuditService,
            ISiteCheckService siteCheckService,
            SiteAuditHubClientService siteAuditHubClient,
            ISettingsService<SiteAuditConfigModel> settingsService,
            ILogger<SiteAuditController> logger,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _siteAuditService = siteAuditService;
            _siteCheckService = siteCheckService;
            _siteAuditHubClient = siteAuditHubClient;
            _settingsService = settingsService;
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
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
            _siteAuditHubClient.AssignClient(Guid.NewGuid().ToString(), auditId);
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
        public IActionResult CreateAudit([FromBody] CreateAuditPostModel postModel)
        {
            //TODO: Move to mapper
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var model = new SiteAuditDto
            {
                Name = postModel.Name,
                CreatedDate = DateTime.UtcNow,
                Status = SiteAuditStatus.Created,
                StartingUrl = new Uri(ctx.UmbracoContext.Content.GetById(postModel.SelectedNodeId).Url(mode: UrlMode.Absolute)),
                SiteChecks = _siteCheckService.GetAll().Where(it => postModel.Checks.Contains(it.Id)).ToList(),
                MaxPagesToCrawl = postModel.MaxPagesToCrawl == 0 ? (int?)null : postModel.MaxPagesToCrawl,
                DelayBetweenRequests = postModel.DelayBetweenRequests * 1000
            };
            model = _siteAuditService.Save(model);
            if (postModel.StartAudit)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var result = _siteAuditService.StartSiteAudit(model).Result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Something went wrong!");
                    }
                });
            }
            return Ok(model.Id);
        }
    }
}
