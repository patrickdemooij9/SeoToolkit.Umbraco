#nullable enable
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Routing;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    /// <summary>
    /// Creating, listing and controlling audit runs.
    /// <para>
    /// Creating one now queues it and returns; the crawl is picked up by a background service.
    /// Previously the controller started the crawl itself on a detached task, so the audit was
    /// tied to the lifetime of a web request that had already been answered.
    /// </para>
    /// </summary>
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit SiteAudit")]
    [BackOfficeRoute("seoToolkitSiteAudit")]
    public class SiteAuditController : SeoToolkitAuthenticatedControllerBase
    {
        private const int MaxPageSize = 500;

        private readonly SiteAuditRunService _runService;
        private readonly SiteAuditViewModelMapper _mapper;
        private readonly ISeoCheckCatalogue _catalogue;
        private readonly ISettingsService<SiteAuditConfigModel> _settingsService;
        private readonly IAuditStartingPointResolver _startingPoints;

        public SiteAuditController(SiteAuditRunService runService,
            SiteAuditViewModelMapper mapper,
            ISeoCheckCatalogue catalogue,
            ISettingsService<SiteAuditConfigModel> settingsService,
            IAuditStartingPointResolver startingPoints)
        {
            _runService = runService;
            _mapper = mapper;
            _catalogue = catalogue;
            _settingsService = settingsService;
            _startingPoints = startingPoints;
        }

        [HttpGet("siteAudits")]
        [ProducesResponseType(typeof(SiteAuditPagedViewModel<SiteAuditRunOverviewViewModel>), 200)]
        public IActionResult GetAll(int skip = 0, int take = 100)
        {
            var page = _runService.GetPaged(skip, Clamp(take));

            return Ok(new SiteAuditPagedViewModel<SiteAuditRunOverviewViewModel>
            {
                Total = page.Total,
                Items = page.Items.Select(_mapper.MapOverview).ToArray()
            });
        }

        [HttpGet("siteAudit")]
        [ProducesResponseType(typeof(SiteAuditRunDetailViewModel), 200)]
        public IActionResult Get(int id)
        {
            var run = _runService.Get(id);
            if (run is null) return NotFound();

            return Ok(_mapper.MapDetail(run));
        }

        /// <summary>
        /// The small payload the workspace reads while a crawl is going. Everything it returns
        /// comes from one row, so watching a crawl costs the same whether it is ten pages or ten
        /// thousand.
        /// </summary>
        [HttpGet("siteAuditStatus")]
        [ProducesResponseType(typeof(SiteAuditRunStatusViewModel), 200)]
        public IActionResult GetStatus(int id)
        {
            var run = _runService.Get(id);
            if (run is null) return NotFound();

            return Ok(_mapper.MapStatus(run));
        }

        [HttpPost("siteAudit")]
        [ProducesResponseType(typeof(int), 200)]
        public IActionResult Create([FromBody] CreateAuditPostModel postModel)
        {
            if (postModel is null) return BadRequest();

            var settings = _settingsService.GetSettings();

            // A node or a url, never both. The url route is what makes a decoupled frontend
            // auditable at all: its routing need not resemble the content tree, so there may be
            // no node whose url points at the page that is actually served.
            var resolved = postModel.SelectedNodeId is { } nodeId && nodeId != Guid.Empty
                ? _startingPoints.ResolveFromNode(nodeId, postModel.Culture)
                : _startingPoints.ResolveFromUrl(postModel.StartingUrl);

            if (!resolved.IsSuccess)
                return resolved.IsNotFound ? NotFound(resolved.Error) : BadRequest(resolved.Error);

            var startingPoint = resolved.StartingPoint!;

            var delaySeconds = settings.AllowMinimumDelayBetweenRequestSetting
                ? Math.Max(postModel.DelayBetweenRequests, settings.MinimumDelayBetweenRequest)
                : settings.MinimumDelayBetweenRequest;

            var run = new AuditRun
            {
                Name = postModel.Name,
                // Already retargeted to the decoupled frontend where the domain declares one, so
                // this is the url that will genuinely be fetched rather than the one Umbraco
                // would serve it from.
                StartingUrl = startingPoint.Url,
                BaseUrl = startingPoint.BaseUrl,
                CreatedUtc = DateTime.UtcNow,
                MaxPages = postModel.MaxPagesToCrawl <= 0 ? null : postModel.MaxPagesToCrawl,
                DelayMs = delaySeconds * 1000,
                ConfigJson = new AuditRunConfig
                {
                    Checks = postModel.Checks ?? Array.Empty<string>(),
                    StartNodeKey = startingPoint.ContentKey,
                    Culture = startingPoint.Culture
                }.ToJson()
            };

            // Queued rather than started here. The job runner picks it up, which is what lets an
            // audit outlive the request and survive a restart.
            var queued = _runService.Queue(run);

            return Ok(queued.Id);
        }

        [HttpPost("stopSiteAudit")]
        public IActionResult Stop([FromBody] StopAuditPostModel model)
        {
            if (model is null) return BadRequest();

            _runService.RequestStop(model.Id);

            // Reported as accepted whether or not the crawl is running on this server: behind a
            // load balancer another one may own it, and the caller cannot do anything differently.
            return Ok();
        }

        [HttpDelete("siteAudit")]
        public IActionResult Delete([FromBody] DeleteAuditsPostModel postModel)
        {
            if (postModel?.Ids is null) return BadRequest();

            foreach (var id in postModel.Ids) _runService.Delete(id);

            return Ok();
        }

        /// <summary>
        /// Describes a node the create screen has been pointed at, so it can offer the languages
        /// it is published in and show which url each one would actually crawl.
        /// </summary>
        [HttpGet("siteAuditStartNode")]
        [ProducesResponseType(typeof(SiteAuditStartNodeViewModel), 200)]
        public IActionResult GetStartNode(Guid nodeId)
        {
            if (nodeId == Guid.Empty) return BadRequest("A node is required.");

            var node = _startingPoints.GetStartNode(nodeId);
            if (node is null) return NotFound($"Content {nodeId} was not found.");

            return Ok(_mapper.MapStartNode(node));
        }

        /// <summary>What the create screen needs: the check catalogue and the delay settings.</summary>
        [HttpGet("siteAuditConfiguration")]
        [ProducesResponseType(typeof(SiteAuditCreateOptionsViewModel), 200)]
        public IActionResult GetConfiguration()
        {
            var settings = _settingsService.GetSettings();

            return Ok(new SiteAuditCreateOptionsViewModel
            {
                Checks = _catalogue.GetAll().Select(_mapper.MapCatalogueEntry).ToArray(),
                AllowMinimumDelayBetweenRequestSetting = settings.AllowMinimumDelayBetweenRequestSetting,
                MinimumDelayBetweenRequest = settings.MinimumDelayBetweenRequest
            });
        }

        internal static int Clamp(int take) => take switch
        {
            <= 0 => 1,
            > MaxPageSize => MaxPageSize,
            _ => take
        };
    }
}
