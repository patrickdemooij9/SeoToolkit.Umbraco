#nullable enable
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels
{
    /// <summary>A run as it appears in the overview list.</summary>
    public class SiteAuditRunOverviewViewModel
    {
        public int Id { get; set; }
        public Guid Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public int TotalCrawled { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int? Score { get; set; }
    }

    /// <summary>
    /// The small payload the backoffice reads while a crawl is going.
    /// <para>
    /// Deliberately tiny. The previous detail endpoint returned every crawled page and every
    /// result, and the workspace fetched it once a second - so the cost of watching a crawl grew
    /// with the size of the crawl. Everything here comes from a single row.
    /// </para>
    /// </summary>
    public class SiteAuditRunStatusViewModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsFinished { get; set; }
        public double Progress { get; set; }
        public int TotalCrawled { get; set; }
        public int TotalDiscovered { get; set; }
        public int CriticalCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int NoticeCount { get; set; }
        public int? Score { get; set; }
    }

    /// <summary>Everything about a run except its results, which are paged separately.</summary>
    public class SiteAuditRunDetailViewModel : SiteAuditRunStatusViewModel
    {
        public Guid Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StartingUrl { get; set; } = string.Empty;
        public string? BaseUrl { get; set; }
        public string CreatedDate { get; set; } = string.Empty;
        public string? StartedDate { get; set; }
        public string? FinishedDate { get; set; }
        public int? MaxPages { get; set; }

        /// <summary>The node the run was aimed at, when it was aimed at one rather than a url.</summary>
        public Guid? StartNodeKey { get; set; }

        public string? Culture { get; set; }
    }

    /// <summary>Per-check totals, which is what the summary panel is built from.</summary>
    public class SiteAuditCheckSummaryViewModel
    {
        public string Alias { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool DidRun { get; set; }
        public int ApplicableCount { get; set; }
        public int FailedCount { get; set; }
        public int IssueCount { get; set; }
        public int Weight { get; set; }
    }

    public class SiteAuditSummaryViewModel
    {
        public IReadOnlyList<SiteAuditCategorySummaryViewModel> Categories { get; set; }
            = Array.Empty<SiteAuditCategorySummaryViewModel>();
    }

    public class SiteAuditCategorySummaryViewModel
    {
        public string Category { get; set; } = string.Empty;
        public int FailedCount { get; set; }
        public int IssueCount { get; set; }
        public IReadOnlyList<SiteAuditCheckSummaryViewModel> Checks { get; set; }
            = Array.Empty<SiteAuditCheckSummaryViewModel>();
    }

    /// <summary>One row in the pages grid.</summary>
    public class SiteAuditResourceViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Kind { get; set; } = string.Empty;
        public int Depth { get; set; }
        public bool IsInternal { get; set; }
        public int ResponseTimeMs { get; set; }
        public long SizeBytes { get; set; }
        public int RedirectCount { get; set; }
        public string? Title { get; set; }
        public string? MetaDescription { get; set; }
        public string? H1 { get; set; }
        public int WordCount { get; set; }
        public bool IsIndexable { get; set; }
        public IReadOnlyList<string> IndexabilityReasons { get; set; } = Array.Empty<string>();
        public string? Failure { get; set; }
        public Guid? UmbracoContentKey { get; set; }
        public int IssueCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }

    public class SiteAuditResourceDetailViewModel : SiteAuditResourceViewModel
    {
        public string? FinalUrl { get; set; }
        public string? ContentType { get; set; }
        public string? Culture { get; set; }
        public IReadOnlyList<SiteAuditIssueViewModel> Issues { get; set; }
            = Array.Empty<SiteAuditIssueViewModel>();
    }

    /// <summary>
    /// One finding. The message is produced at read time from the stored data, so wording can
    /// change - or be translated - without touching what was recorded.
    /// </summary>
    public class SiteAuditIssueViewModel
    {
        public int Id { get; set; }
        public string CheckAlias { get; set; } = string.Empty;
        public string CheckName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Variant { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public bool IsWarning { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? Evidence { get; set; }
        public int? ResourceId { get; set; }
        public IReadOnlyDictionary<string, string> Data { get; set; } = new Dictionary<string, string>(0);
    }

    /// <summary>A check as offered when configuring an audit.</summary>
    public class SiteAuditCheckCatalogueViewModel
    {
        public string Alias { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string DefaultSeverity { get; set; } = string.Empty;
        public int Weight { get; set; }
        public bool SupportsSinglePage { get; set; }
        public string? DocumentationUrl { get; set; }
        public string? ProviderName { get; set; }

        /// <summary>The feature this check belongs to, when it is not part of the base package.</summary>
        public string? RequiresFeature { get; set; }

        /// <summary>False when the check is listed but cannot run here.</summary>
        public bool IsAvailable { get; set; }

        public IReadOnlyList<SiteAuditCheckOptionViewModel> Options { get; set; }
            = Array.Empty<SiteAuditCheckOptionViewModel>();
    }

    public class SiteAuditCheckOptionViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public object? DefaultValue { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
    }

    /// <summary>Settings the create screen needs.</summary>
    public class SiteAuditCreateOptionsViewModel
    {
        public IReadOnlyList<SiteAuditCheckCatalogueViewModel> Checks { get; set; }
            = Array.Empty<SiteAuditCheckCatalogueViewModel>();

        public bool AllowMinimumDelayBetweenRequestSetting { get; set; }
        public int MinimumDelayBetweenRequest { get; set; }
    }

    /// <summary>One language a start node is published in.</summary>
    public class SiteAuditStartNodeCultureViewModel
    {
        public string IsoCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>The url the crawl would start from for this language, BaseUrl already applied.</summary>
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>A node the create screen has been pointed at.</summary>
    public class SiteAuditStartNodeViewModel
    {
        public Guid Key { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>True when a language has to be chosen before the crawl url is decided.</summary>
        public bool VariesByCulture { get; set; }

        /// <summary>The single url for a node that does not vary. Null when it does.</summary>
        public string? Url { get; set; }

        public IReadOnlyList<SiteAuditStartNodeCultureViewModel> Cultures { get; set; }
            = Array.Empty<SiteAuditStartNodeCultureViewModel>();
    }

    /// <summary>The outcome of checking one page from the content app.</summary>
    public class SiteAuditPageCheckResultViewModel
    {
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Only failures are listed. A check that raised nothing passed, which is what lets the
        /// content app show a tick without the server having to report every non-finding.
        /// </summary>
        public IReadOnlyList<SiteAuditIssueViewModel> Issues { get; set; }
            = Array.Empty<SiteAuditIssueViewModel>();
    }

    /// <summary>A page of results.</summary>
    public class SiteAuditPagedViewModel<T>
    {
        public long Total { get; set; }
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    }
}
