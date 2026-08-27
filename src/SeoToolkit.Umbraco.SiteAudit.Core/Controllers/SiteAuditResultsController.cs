#nullable enable
using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Routing;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    /// <summary>
    /// Reading the results of a run.
    /// <para>
    /// Everything here is paged and filtered on the server. The previous detail endpoint returned
    /// every crawled page and every result in one payload and the client sliced it ten at a time,
    /// which put a hard ceiling on how large a site could usefully be audited.
    /// </para>
    /// </summary>
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit SiteAudit")]
    [BackOfficeRoute("seoToolkitSiteAudit")]
    public class SiteAuditResultsController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly SiteAuditRunService _runService;
        private readonly SiteAuditViewModelMapper _mapper;

        public SiteAuditResultsController(SiteAuditRunService runService, SiteAuditViewModelMapper mapper)
        {
            _runService = runService;
            _mapper = mapper;
        }

        [HttpGet("siteAuditResources")]
        [ProducesResponseType(typeof(SiteAuditPagedViewModel<SiteAuditResourceViewModel>), 200)]
        public IActionResult GetResources(int runId,
            int skip = 0,
            int take = 50,
            string? checkAlias = null,
            string? severity = null,
            int? statusCode = null,
            int? statusClass = null,
            string? kind = null,
            bool? hasIssues = null,
            string? search = null,
            string? sort = null,
            bool descending = false)
        {
            var filter = new AuditResourceFilter
            {
                CheckAlias = checkAlias,
                MinimumSeverity = ParseSeverity(severity),
                StatusCode = statusCode,
                StatusClass = statusClass,
                Kind = ParseEnum<ResourceKind>(kind),
                HasIssues = hasIssues,
                UrlContains = search,
                Sort = ParseEnum<AuditResourceSort>(sort) ?? AuditResourceSort.Url,
                Descending = descending
            };

            var page = _runService.GetResources(runId, skip, SiteAuditController.Clamp(take), filter);

            return Ok(new SiteAuditPagedViewModel<SiteAuditResourceViewModel>
            {
                Total = page.Total,
                Items = page.Items.Select(_mapper.MapResource).ToArray()
            });
        }

        [HttpGet("siteAuditResource")]
        [ProducesResponseType(typeof(SiteAuditResourceDetailViewModel), 200)]
        public IActionResult GetResource(int runId, int resourceId)
        {
            var resource = _runService.GetResource(runId, resourceId);
            if (resource is null) return NotFound();

            var issues = _runService.GetIssuesForResource(runId, resourceId);

            return Ok(_mapper.MapResourceDetail(resource, issues));
        }

        [HttpGet("siteAuditIssues")]
        [ProducesResponseType(typeof(SiteAuditPagedViewModel<SiteAuditIssueViewModel>), 200)]
        public IActionResult GetIssues(int runId,
            int skip = 0,
            int take = 50,
            string? checkAlias = null,
            string? severity = null)
        {
            var filter = new AuditIssueFilter
            {
                CheckAlias = checkAlias,
                Severity = ParseSeverity(severity)
            };

            var page = _runService.GetIssues(runId, skip, SiteAuditController.Clamp(take), filter);

            return Ok(new SiteAuditPagedViewModel<SiteAuditIssueViewModel>
            {
                Total = page.Total,
                Items = page.Items.Select(_mapper.MapIssue).ToArray()
            });
        }

        /// <summary>
        /// Per-check totals grouped by category. Read from the stored aggregates, so it never
        /// touches the results tables however large the run was.
        /// </summary>
        [HttpGet("siteAuditSummary")]
        [ProducesResponseType(typeof(SiteAuditSummaryViewModel), 200)]
        public IActionResult GetSummary(int runId)
            => Ok(_mapper.MapSummary(_runService.GetCheckRuns(runId)));

        /// <summary>
        /// Every finding as csv. Streamed straight out rather than paged: an export that stops
        /// after the first page is worse than no export at all.
        /// </summary>
        [HttpGet("siteAuditExport")]
        public IActionResult Export(int runId)
        {
            var run = _runService.Get(runId);
            if (run is null) return NotFound();

            var csv = new CsvWriter();
            csv.Row("Severity", "Check", "Category", "Url", "Message", "Evidence");

            const int pageSize = 500;
            var skip = 0;

            while (true)
            {
                var page = _runService.GetIssues(runId, skip, pageSize);
                var items = page.Items.ToArray();
                if (items.Length == 0) break;

                foreach (var issue in items.Select(_mapper.MapIssue))
                {
                    csv.Row(issue.Severity, issue.CheckName, issue.Category,
                        issue.Url, issue.Message, issue.Evidence);
                }

                skip += pageSize;
                if (skip >= page.Total) break;
            }

            var fileName = $"site-audit-{runId}-{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(csv.ToBytes(), "text/csv", fileName);
        }

        private static SeoSeverity? ParseSeverity(string? value) => ParseEnum<SeoSeverity>(value);

        private static T? ParseEnum<T>(string? value) where T : struct, Enum
            => Enum.TryParse<T>(value, ignoreCase: true, out var parsed) ? parsed : null;
    }
}
