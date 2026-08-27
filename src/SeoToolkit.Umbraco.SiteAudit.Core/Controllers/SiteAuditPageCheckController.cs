#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Routing;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    /// <summary>
    /// Checking a single page from the content app, so an editor can get an answer about the
    /// page they are working on without running a whole audit.
    /// </summary>
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit SiteAudit")]
    [BackOfficeRoute("seoToolkitSiteAudit")]
    public class SiteAuditPageCheckController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly SiteAuditRunService _runService;
        private readonly SiteAuditViewModelMapper _mapper;
        private readonly ISeoCheckCatalogue _catalogue;
        private readonly IAuditStartingPointResolver _startingPoints;

        public SiteAuditPageCheckController(SiteAuditRunService runService,
            SiteAuditViewModelMapper mapper,
            ISeoCheckCatalogue catalogue,
            IAuditStartingPointResolver startingPoints)
        {
            _runService = runService;
            _mapper = mapper;
            _catalogue = catalogue;
            _startingPoints = startingPoints;
        }

        /// <summary>The checks that make sense against a single page in isolation.</summary>
        [HttpGet("pageChecks")]
        [ProducesResponseType(typeof(SiteAuditCheckCatalogueViewModel[]), 200)]
        public IActionResult GetPageChecks()
            => Ok(_catalogue.GetAll()
                .Where(it => it.SupportsSinglePage)
                .Select(_mapper.MapCatalogueEntry)
                .ToArray());

        [HttpPost("run")]
        [ProducesResponseType(typeof(SiteAuditPageCheckResultViewModel), 200)]
        public async Task<IActionResult> RunPageChecks([FromBody] RunPageCheckPostModel postModel)
        {
            if (postModel is null) return BadRequest();

            // Resolved the same way an audit's starting point is, so a decoupled site checks the
            // page its visitors see rather than the one Umbraco would serve.
            var resolved = _startingPoints.ResolveFromNode(postModel.ContentId, postModel.Culture);
            if (!resolved.IsSuccess)
                return resolved.IsNotFound ? NotFound(resolved.Error) : BadRequest(resolved.Error);

            var url = resolved.StartingPoint!.Url;

            var issues = await _runService.RunSinglePageAsync(url, HttpContext.RequestAborted);

            return Ok(new SiteAuditPageCheckResultViewModel
            {
                Url = url.ToString(),
                Issues = issues.Select(_mapper.MapIssue).ToArray()
            });
        }
    }
}
