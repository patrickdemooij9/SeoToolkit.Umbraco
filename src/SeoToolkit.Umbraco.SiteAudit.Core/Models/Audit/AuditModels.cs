#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit
{
    /// <summary>
    /// An audit run as the rest of the application sees it.
    /// <para>
    /// Deliberately does not carry its resources or issues. The old model held every crawled page
    /// in a queue on the run itself, which is why opening one audit loaded the entire database.
    /// Results are paged out of the repository instead.
    /// </para>
    /// </summary>
    public sealed class AuditRun
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public SiteAuditStatus Status { get; set; }

        public DateTime CreatedUtc { get; set; }
        public DateTime? StartedUtc { get; set; }
        public DateTime? FinishedUtc { get; set; }

        public required Uri StartingUrl { get; set; }

        /// <summary>The decoupled frontend this run was pointed at, when there is one.</summary>
        public string? BaseUrl { get; set; }

        public int? MaxPages { get; set; }
        public int? MaxDepth { get; set; }
        public int DelayMs { get; set; }
        public int Concurrency { get; set; }
        public string? UserAgent { get; set; }

        public int TotalDiscovered { get; set; }
        public int TotalCrawled { get; set; }

        public int CriticalCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int NoticeCount { get; set; }

        public int? Score { get; set; }
        public int ScoreVersion { get; set; }

        public DateTime? HeartbeatUtc { get; set; }
        public string? ClaimedBy { get; set; }
        public string? ConfigJson { get; set; }
        public int? ScheduleId { get; set; }

        public bool IsRunning => Status == SiteAuditStatus.Running;

        /// <summary>Whether the run has reached a state it will not move on from by itself.</summary>
        public bool IsFinished => Status is SiteAuditStatus.Finished
            or SiteAuditStatus.Error
            or SiteAuditStatus.Stopped
            or SiteAuditStatus.Interrupted;

        /// <summary>
        /// How far along the crawl is, as a percentage. Based on the counts stored on the run,
        /// so reporting progress never has to touch the results tables.
        /// </summary>
        public double Progress
        {
            get
            {
                if (IsFinished) return 100;
                if (Status != SiteAuditStatus.Running) return 0;

                // Whichever bound is tighter. A limit set higher than the site is large leaves
                // the bar short of the end for the whole last stretch of the crawl, and a site
                // bigger than the limit would never fill it at all.
                var target = MaxPages.HasValue && TotalDiscovered > 0
                    ? Math.Min(MaxPages.Value, TotalDiscovered)
                    : MaxPages ?? TotalDiscovered;

                if (target <= 0) return 0;

                return Math.Min(100d, (double)TotalCrawled / target * 100d);
            }
        }
    }

    /// <summary>One crawled resource, as stored.</summary>
    public sealed class AuditResource
    {
        public int Id { get; set; }
        public int RunId { get; set; }
        public string UrlHash { get; set; } = string.Empty;
        public required Uri Url { get; set; }
        public Uri? FinalUrl { get; set; }
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public ResourceKind Kind { get; set; }
        public int Depth { get; set; }
        public bool IsInternal { get; set; }
        public int ResponseTimeMs { get; set; }
        public long SizeBytes { get; set; }
        public int RedirectCount { get; set; }
        public string? Title { get; set; }
        public string? MetaDescription { get; set; }
        public string? H1 { get; set; }
        public int WordCount { get; set; }
        public IndexabilityFlags Indexability { get; set; }
        public CrawlFailureReason? Failure { get; set; }
        public Guid? UmbracoContentKey { get; set; }
        public string? Culture { get; set; }
        public int IssueCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }

    /// <summary>One finding, as stored.</summary>
    public sealed class AuditIssue
    {
        public int Id { get; set; }
        public int RunId { get; set; }
        public int? ResourceId { get; set; }
        public string CheckAlias { get; set; } = string.Empty;
        public string? Variant { get; set; }
        public SeoSeverity Severity { get; set; }
        public Uri? Url { get; set; }
        public IReadOnlyDictionary<string, string> Data { get; set; } = new Dictionary<string, string>(0);
        public string? Evidence { get; set; }
        public string? IssueHash { get; set; }
    }

    /// <summary>What one check did during a run.</summary>
    public sealed class AuditCheckRun
    {
        public int RunId { get; set; }
        public string CheckAlias { get; set; } = string.Empty;
        public string? CheckName { get; set; }
        public SeoCheckCategory Category { get; set; }
        public bool DidRun { get; set; }
        public int ApplicableCount { get; set; }
        public int FailedCount { get; set; }
        public int IssueCount { get; set; }
        public int Weight { get; set; }
        public SeoSeverity Severity { get; set; }
        public long DurationMs { get; set; }
    }

    /// <summary>Which resources to return, translated into a where clause by the repository.</summary>
    public sealed class AuditResourceFilter
    {
        public SeoSeverity? MinimumSeverity { get; init; }
        public string? CheckAlias { get; init; }
        public int? StatusCode { get; init; }

        /// <summary>Matches a whole class of status, e.g. 4 for any 4xx.</summary>
        public int? StatusClass { get; init; }

        public ResourceKind? Kind { get; init; }
        public bool? IsInternal { get; init; }
        public bool? HasIssues { get; init; }
        public string? UrlContains { get; init; }
        public AuditResourceSort Sort { get; init; } = AuditResourceSort.Url;
        public bool Descending { get; init; }
    }

    public enum AuditResourceSort
    {
        Url = 0,
        StatusCode = 1,
        IssueCount = 2,
        ResponseTime = 3,
        Depth = 4,
        WordCount = 5,
        SizeBytes = 6
    }

    /// <summary>Which issues to return.</summary>
    public sealed class AuditIssueFilter
    {
        public string? CheckAlias { get; init; }
        public SeoSeverity? MinimumSeverity { get; init; }
        public SeoSeverity? Severity { get; init; }
        public int? ResourceId { get; init; }
    }
}
