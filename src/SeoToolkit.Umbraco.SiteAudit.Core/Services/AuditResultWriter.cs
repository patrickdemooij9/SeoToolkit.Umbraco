#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Infrastructure.Scoping;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    /// <summary>
    /// Writes crawl results as they arrive.
    /// <para>
    /// Batched deliberately. The previous implementation opened a scope and inserted a row for
    /// every single crawled page, which on a large site meant thousands of separate transactions.
    /// Buffering also keeps memory bounded: the buffer is flushed and released rather than
    /// accumulating a run's worth of results before anything is written.
    /// </para>
    /// <para>
    /// Not thread safe by itself - the crawler raises its completion event from several worker
    /// threads, so <see cref="Handle"/> takes a lock. The work inside it is small compared with
    /// fetching a page, so this does not become the bottleneck.
    /// </para>
    /// </summary>
    public sealed class AuditResultWriter
    {
        private readonly int _runId;
        private readonly ISiteAuditRunRepository _repository;
        private readonly IScopeProvider _scopeProvider;
        private readonly int _batchSize;
        private readonly object _lock = new();

        private readonly List<AuditResourceWrite> _buffer = new();

        private int _totalCrawled;
        private int _totalDiscovered;
        private int _criticalCount;
        private int _errorCount;
        private int _warningCount;
        private int _noticeCount;

        public AuditResultWriter(int runId,
            ISiteAuditRunRepository repository,
            IScopeProvider scopeProvider,
            int batchSize = SeoToolkit.Umbraco.SiteAudit.Core.Repositories.SiteAuditRunRepository.BatchSize)
        {
            _runId = runId;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            _batchSize = batchSize < 1 ? 1 : batchSize;
        }

        public int TotalCrawled => _totalCrawled;

        /// <summary>Hook this to <see cref="CrawlEngine.ResourceCrawled"/>.</summary>
        public void Handle(object? sender, ResourceCrawledEventArgs args)
        {
            if (args is null) return;

            List<AuditResourceWrite>? toFlush = null;

            lock (_lock)
            {
                _buffer.Add(new AuditResourceWrite
                {
                    Resource = MapResource(args.Resource, args.Summary),
                    Issues = args.Issues.Select(MapIssue).ToArray()
                });

                _totalCrawled = args.Completed;
                _totalDiscovered = args.Discovered;

                foreach (var issue in args.Issues) CountSeverity(issue.Severity);

                if (_buffer.Count >= _batchSize)
                {
                    toFlush = new List<AuditResourceWrite>(_buffer);
                    _buffer.Clear();
                }
            }

            // Outside the lock: writing to the database must not hold up the crawl workers.
            if (toFlush is not null) Write(toFlush);
        }

        /// <summary>
        /// Writes anything still buffered, then the site-wide findings and per-check totals.
        /// </summary>
        public void Complete(CrawlResult result)
        {
            if (result is null) throw new ArgumentNullException(nameof(result));

            List<AuditResourceWrite> remaining;
            lock (_lock)
            {
                remaining = new List<AuditResourceWrite>(_buffer);
                _buffer.Clear();
            }

            if (remaining.Count > 0) Write(remaining);

            // Findings with no resource belong to the site as a whole, and are raised only once
            // the crawl has finished, so they are written here rather than per batch.
            var siteIssues = result.Issues.Where(it => it.Url is null).Select(MapIssue).ToArray();

            using var scope = _scopeProvider.CreateScope();

            if (siteIssues.Length > 0)
            {
                foreach (var issue in siteIssues) CountSeverity(issue.Severity);
                _repository.AddSiteIssues(_runId, siteIssues);
            }

            _repository.SaveCheckRuns(_runId, result.CheckStats.Select(MapCheckRun).ToArray());

            scope.Complete();
        }

        /// <summary>Applies the totals gathered during the crawl to the run itself.</summary>
        public void ApplyTotals(AuditRun run, CrawlResult result)
        {
            if (run is null) throw new ArgumentNullException(nameof(run));

            run.TotalCrawled = result?.Crawled ?? _totalCrawled;
            run.TotalDiscovered = result?.Discovered ?? _totalDiscovered;
            run.CriticalCount = _criticalCount;
            run.ErrorCount = _errorCount;
            run.WarningCount = _warningCount;
            run.NoticeCount = _noticeCount;
        }

        public void Heartbeat()
        {
            using var scope = _scopeProvider.CreateScope();
            _repository.UpdateHeartbeat(_runId, _totalCrawled, _totalDiscovered);
            scope.Complete();
        }

        private void Write(IReadOnlyList<AuditResourceWrite> batch)
        {
            using var scope = _scopeProvider.CreateScope();
            _repository.AddResults(_runId, batch);
            scope.Complete();
        }

        private void CountSeverity(SeoSeverity severity)
        {
            switch (severity)
            {
                case SeoSeverity.Critical: _criticalCount++; break;
                case SeoSeverity.Error: _errorCount++; break;
                case SeoSeverity.Warning: _warningCount++; break;
                case SeoSeverity.Notice: _noticeCount++; break;
            }
        }

        public static AuditResource MapResource(CrawledResource resource, ResourceSummary summary) => new()
        {
            Indexability = summary.Indexability,
            UrlHash = resource.UrlHash,
            Url = resource.RequestedUrl,
            FinalUrl = resource.FinalUrl,
            StatusCode = resource.StatusCode,
            ContentType = resource.ContentType,
            Kind = resource.Kind,
            Depth = resource.Depth,
            IsInternal = resource.IsInternal,
            ResponseTimeMs = resource.TotalMs,
            SizeBytes = resource.TransferredBytes,
            RedirectCount = resource.RedirectChain.Count,
            Title = resource.Facts?.Title,
            MetaDescription = resource.Facts?.MetaDescription,
            H1 = resource.Facts?.PrimaryH1,
            WordCount = resource.Facts?.WordCount ?? 0,
            Failure = resource.Failure,
            UmbracoContentKey = resource.UmbracoContentKey,
            Culture = resource.Culture
        };

        public static AuditIssue MapIssue(SeoCheckIssue issue) => new()
        {
            CheckAlias = issue.CheckAlias,
            Variant = issue.Variant,
            Severity = issue.Severity,
            Url = issue.Url,
            Data = issue.Data,
            Evidence = issue.Evidence,
            IssueHash = issue.IssueHash
        };

        public static AuditCheckRun MapCheckRun(CheckRunStats stats) => new()
        {
            CheckAlias = stats.Alias,
            CheckName = stats.Name,
            Category = stats.Category,
            DidRun = stats.DidRun,
            ApplicableCount = stats.ApplicableCount,
            FailedCount = stats.FailedCount,
            IssueCount = stats.IssueCount,
            Weight = stats.Weight,
            Severity = stats.Severity,
            DurationMs = stats.DurationMs
        };
    }
}
