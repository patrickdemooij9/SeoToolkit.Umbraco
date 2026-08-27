#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Database;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Repositories
{
    /// <summary>
    /// Stores audit runs and results.
    /// <para>
    /// Registered scoped rather than singleton: it reads the ambient scope for its database, and
    /// a singleton holding a scope accessor was only ever working by accident.
    /// </para>
    /// </summary>
    public class SiteAuditRunRepository : ISiteAuditRunRepository
    {
        /// <summary>
        /// How many rows go in one insert. Large enough that a big crawl is not thousands of
        /// round trips, small enough that a failure loses very little and memory stays flat.
        /// </summary>
        public const int BatchSize = 200;

        private readonly IScopeAccessor _scopeAccessor;

        public SiteAuditRunRepository(IScopeAccessor scopeAccessor)
        {
            _scopeAccessor = scopeAccessor;
        }

        private IScope AmbientScope => _scopeAccessor.AmbientScope
            ?? throw new InvalidOperationException("A scope is required to use the site audit repository.");

        private IUmbracoDatabase Database => AmbientScope.Database;

        private Sql<ISqlContext> Sql() => AmbientScope.SqlContext.Sql();

        // ---- runs ---------------------------------------------------------------------------

        public AuditRun? Get(int id)
        {
            var entity = Database.SingleOrDefault<SiteAuditRunEntity>(
                Sql().SelectAll().From<SiteAuditRunEntity>().Where<SiteAuditRunEntity>(it => it.Id == id));

            return entity is null ? null : Map(entity);
        }

        public AuditRun? Get(Guid key)
        {
            var entity = Database.SingleOrDefault<SiteAuditRunEntity>(
                Sql().SelectAll().From<SiteAuditRunEntity>().Where<SiteAuditRunEntity>(it => it.Key == key));

            return entity is null ? null : Map(entity);
        }

        public PagedModel<AuditRun> GetPaged(int skip, int take)
        {
            var sql = Sql().SelectAll().From<SiteAuditRunEntity>().OrderByDescending<SiteAuditRunEntity>(it => it.Id);
            var page = Database.SkipTake<SiteAuditRunEntity>(skip, take, sql);
            var total = Database.ExecuteScalar<long>(Sql().SelectCount().From<SiteAuditRunEntity>());

            return new PagedModel<AuditRun>(total, page.Select(Map).ToArray());
        }

        public AuditRun Save(AuditRun run)
        {
            if (run is null) throw new ArgumentNullException(nameof(run));

            var entity = Map(run);

            if (run.Id == 0)
            {
                if (entity.Key == Guid.Empty) entity.Key = Guid.NewGuid();
                Database.Insert(entity);
                run.Id = entity.Id;
                run.Key = entity.Key;
            }
            else
            {
                Database.Update(entity);
            }

            return run;
        }

        public void Delete(int id)
        {
            // Children first: the results tables carry a foreign key back to the run.
            Database.Execute($"DELETE FROM [{SiteAuditIssueEntity.TableName}] WHERE [RunId] = @0", id);
            Database.Execute($"DELETE FROM [{SiteAuditResourceEntity.TableName}] WHERE [RunId] = @0", id);
            Database.Execute($"DELETE FROM [{SiteAuditCheckRunEntity.TableName}] WHERE [RunId] = @0", id);
            Database.Execute($"DELETE FROM [{SiteAuditRunEntity.TableName}] WHERE [Id] = @0", id);
        }

        public int DeleteAllButNewest(int keep)
        {
            if (keep < 0) throw new ArgumentOutOfRangeException(nameof(keep));

            var doomed = Database.Fetch<int>(Sql()
                .Select<SiteAuditRunEntity>(it => it.Id)
                .From<SiteAuditRunEntity>()
                .OrderByDescending<SiteAuditRunEntity>(it => it.Id))
                .Skip(keep)
                .ToArray();

            foreach (var id in doomed) Delete(id);

            return doomed.Length;
        }

        public IReadOnlyList<AuditRun> GetQueued(int max)
        {
            var sql = Sql().SelectAll()
                .From<SiteAuditRunEntity>()
                .Where<SiteAuditRunEntity>(it => it.StatusId == (int)SiteAuditStatus.Scheduled)
                .OrderBy<SiteAuditRunEntity>(it => it.Id);

            return Database.SkipTake<SiteAuditRunEntity>(0, max, sql).Select(Map).ToArray();
        }

        public bool TryClaim(int runId, string claimedBy)
        {
            // A conditional update is the claim. Two servers ticking at the same moment cannot
            // both succeed, because only one of them will still see the Scheduled status.
            var affected = Database.Execute(
                $"UPDATE [{SiteAuditRunEntity.TableName}] " +
                "SET [StatusId] = @0, [ClaimedBy] = @1, [StartedUtc] = @2, [HeartbeatUtc] = @2 " +
                "WHERE [Id] = @3 AND [StatusId] = @4",
                (int)SiteAuditStatus.Running, claimedBy, DateTime.UtcNow, runId, (int)SiteAuditStatus.Scheduled);

            return affected == 1;
        }

        public void UpdateHeartbeat(int runId, int totalCrawled, int totalDiscovered)
            => Database.Execute(
                $"UPDATE [{SiteAuditRunEntity.TableName}] " +
                "SET [HeartbeatUtc] = @0, [TotalCrawled] = @1, [TotalDiscovered] = @2 WHERE [Id] = @3",
                DateTime.UtcNow, totalCrawled, totalDiscovered, runId);

        public int MarkStaleRunsInterrupted(TimeSpan staleAfter)
            => Database.Execute(
                $"UPDATE [{SiteAuditRunEntity.TableName}] " +
                "SET [StatusId] = @0, [FinishedUtc] = @1 " +
                "WHERE [StatusId] = @2 AND ([HeartbeatUtc] IS NULL OR [HeartbeatUtc] < @3)",
                (int)SiteAuditStatus.Interrupted, DateTime.UtcNow, (int)SiteAuditStatus.Running,
                DateTime.UtcNow.Subtract(staleAfter));

        // ---- writing results ----------------------------------------------------------------

        public void AddResults(int runId, IReadOnlyList<AuditResourceWrite> resources)
        {
            if (resources is null || resources.Count == 0) return;

            var issues = new List<SiteAuditIssueEntity>();

            foreach (var write in resources)
            {
                var entity = Map(runId, write.Resource, write.Issues);

                // Inserted one at a time on purpose. A bulk insert does not reliably hand back
                // generated identity values - on SQL Server it goes through bulk copy, which
                // returns none - and every issue needs its resource id. Silently writing issues
                // with a resource id of zero would be far worse than the extra round trips, and
                // the whole batch still shares a single transaction.
                Database.Insert(entity);

                foreach (var issue in write.Issues)
                    issues.Add(Map(runId, entity.Id, issue));
            }

            if (issues.Count > 0) Database.InsertBulk(issues);
        }

        public void AddSiteIssues(int runId, IReadOnlyList<AuditIssue> issues)
        {
            if (issues is null || issues.Count == 0) return;

            Database.InsertBulk(issues.Select(it => Map(runId, null, it)).ToArray());
        }

        public void SaveCheckRuns(int runId, IReadOnlyList<AuditCheckRun> checkRuns)
        {
            if (checkRuns is null || checkRuns.Count == 0) return;

            Database.Execute($"DELETE FROM [{SiteAuditCheckRunEntity.TableName}] WHERE [RunId] = @0", runId);
            Database.InsertBulk(checkRuns.Select(it => Map(runId, it)).ToArray());
        }

        public IReadOnlyList<AuditCheckRun> GetCheckRuns(int runId)
            => Database.Fetch<SiteAuditCheckRunEntity>(Sql()
                    .SelectAll()
                    .From<SiteAuditCheckRunEntity>()
                    .Where<SiteAuditCheckRunEntity>(it => it.RunId == runId))
                .Select(Map)
                .ToArray();

        // ---- reading results ------------------------------------------------------------------

        public PagedModel<AuditResource> GetResources(int runId, int skip, int take, AuditResourceFilter? filter = null)
        {
            var sql = Sql().SelectAll().From<SiteAuditResourceEntity>()
                .Where<SiteAuditResourceEntity>(it => it.RunId == runId);

            var countSql = Sql().SelectCount().From<SiteAuditResourceEntity>()
                .Where<SiteAuditResourceEntity>(it => it.RunId == runId);

            if (filter is not null)
            {
                ApplyResourceFilter(sql, filter, runId);
                ApplyResourceFilter(countSql, filter, runId);
            }

            ApplyResourceSort(sql, filter ?? new AuditResourceFilter());

            var total = Database.ExecuteScalar<long>(countSql);
            var page = Database.SkipTake<SiteAuditResourceEntity>(skip, take, sql);

            return new PagedModel<AuditResource>(total, page.Select(Map).ToArray());
        }

        public AuditResource? GetResource(int runId, int resourceId)
        {
            var entity = Database.SingleOrDefault<SiteAuditResourceEntity>(Sql()
                .SelectAll()
                .From<SiteAuditResourceEntity>()
                .Where<SiteAuditResourceEntity>(it => it.RunId == runId && it.Id == resourceId));

            return entity is null ? null : Map(entity);
        }

        public PagedModel<AuditIssue> GetIssues(int runId, int skip, int take, AuditIssueFilter? filter = null)
        {
            var sql = Sql().SelectAll().From<SiteAuditIssueEntity>()
                .Where<SiteAuditIssueEntity>(it => it.RunId == runId);

            var countSql = Sql().SelectCount().From<SiteAuditIssueEntity>()
                .Where<SiteAuditIssueEntity>(it => it.RunId == runId);

            if (filter is not null)
            {
                ApplyIssueFilter(sql, filter);
                ApplyIssueFilter(countSql, filter);
            }

            // Worst first, then a stable tiebreaker. Both columns go in one clause - see the note
            // on SqlOrderBy for why a second OrderBy call cannot be used here.
            sql.OrderBy(SqlOrderBy.Clause("Severity DESC", "Id"));

            var total = Database.ExecuteScalar<long>(countSql);
            var page = Database.SkipTake<SiteAuditIssueEntity>(skip, take, sql);

            return new PagedModel<AuditIssue>(total, page.Select(Map).ToArray());
        }

        public IReadOnlyList<AuditIssue> GetIssuesForResource(int runId, int resourceId)
            => Database.Fetch<SiteAuditIssueEntity>(Sql()
                    .SelectAll()
                    .From<SiteAuditIssueEntity>()
                    .Where<SiteAuditIssueEntity>(it => it.RunId == runId && it.ResourceId == resourceId))
                .Select(Map)
                .ToArray();

        private static void ApplyResourceFilter(Sql<ISqlContext> sql, AuditResourceFilter filter, int runId)
        {
            if (filter.StatusCode.HasValue)
                sql.Where<SiteAuditResourceEntity>(it => it.StatusCode == filter.StatusCode.Value);

            if (filter.StatusClass.HasValue)
            {
                var lower = filter.StatusClass.Value * 100;
                var upper = lower + 99;
                sql.Where<SiteAuditResourceEntity>(it => it.StatusCode >= lower && it.StatusCode <= upper);
            }

            if (filter.Kind.HasValue)
            {
                var kindId = (int)filter.Kind.Value;
                sql.Where<SiteAuditResourceEntity>(it => it.KindId == kindId);
            }

            if (filter.IsInternal.HasValue)
                sql.Where<SiteAuditResourceEntity>(it => it.IsInternal == filter.IsInternal.Value);

            if (filter.HasIssues == true)
                sql.Where<SiteAuditResourceEntity>(it => it.IssueCount > 0);
            else if (filter.HasIssues == false)
                sql.Where<SiteAuditResourceEntity>(it => it.IssueCount == 0);

            if (!string.IsNullOrWhiteSpace(filter.UrlContains))
                sql.Where("[Url] LIKE @0", "%" + filter.UrlContains + "%");

            if (filter.MinimumSeverity == SeoSeverity.Error)
                sql.Where<SiteAuditResourceEntity>(it => it.ErrorCount > 0);
            else if (filter.MinimumSeverity == SeoSeverity.Warning)
                sql.Where<SiteAuditResourceEntity>(it => it.ErrorCount > 0 || it.WarningCount > 0);

            // Filtering by check has to reach into the issues table, but only as an EXISTS -
            // a join would multiply the resource rows by their issues and break paging.
            if (!string.IsNullOrWhiteSpace(filter.CheckAlias))
            {
                sql.Where(
                    $"EXISTS (SELECT 1 FROM [{SiteAuditIssueEntity.TableName}] i " +
                    $"WHERE i.[ResourceId] = [{SiteAuditResourceEntity.TableName}].[Id] " +
                    "AND i.[RunId] = @0 AND i.[CheckAlias] = @1)",
                    runId, filter.CheckAlias);
            }
        }

        private static void ApplyResourceSort(Sql<ISqlContext> sql, AuditResourceFilter filter)
        {
            var column = filter.Sort switch
            {
                AuditResourceSort.StatusCode => nameof(SiteAuditResourceEntity.StatusCode),
                AuditResourceSort.IssueCount => nameof(SiteAuditResourceEntity.IssueCount),
                AuditResourceSort.ResponseTime => nameof(SiteAuditResourceEntity.ResponseTimeMs),
                AuditResourceSort.Depth => nameof(SiteAuditResourceEntity.Depth),
                AuditResourceSort.WordCount => nameof(SiteAuditResourceEntity.WordCount),
                AuditResourceSort.SizeBytes => nameof(SiteAuditResourceEntity.SizeBytes),
                _ => nameof(SiteAuditResourceEntity.Url)
            };

            // Paging over a non-unique sort column is not stable on its own, so ties break on the
            // primary key. Without it a row can appear on two pages and another on none.
            sql.OrderBy(SqlOrderBy.Clause(
                filter.Descending ? column + " DESC" : column,
                nameof(SiteAuditResourceEntity.Id)));
        }

        private static void ApplyIssueFilter(Sql<ISqlContext> sql, AuditIssueFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.CheckAlias))
                sql.Where<SiteAuditIssueEntity>(it => it.CheckAlias == filter.CheckAlias);

            if (filter.Severity.HasValue)
            {
                var severity = (int)filter.Severity.Value;
                sql.Where<SiteAuditIssueEntity>(it => it.Severity == severity);
            }

            if (filter.MinimumSeverity.HasValue)
            {
                var minimum = (int)filter.MinimumSeverity.Value;
                sql.Where<SiteAuditIssueEntity>(it => it.Severity >= minimum);
            }

            if (filter.ResourceId.HasValue)
                sql.Where<SiteAuditIssueEntity>(it => it.ResourceId == filter.ResourceId.Value);
        }

        private static AuditRun Map(SiteAuditRunEntity entity) => new()
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name,
            Status = (SiteAuditStatus)entity.StatusId,
            CreatedUtc = entity.CreatedUtc,
            StartedUtc = entity.StartedUtc,
            FinishedUtc = entity.FinishedUtc,
            StartingUrl = new Uri(entity.StartingUrl),
            BaseUrl = entity.BaseUrl,
            MaxPages = entity.MaxPages,
            MaxDepth = entity.MaxDepth,
            DelayMs = entity.DelayMs,
            Concurrency = entity.Concurrency,
            UserAgent = entity.UserAgent,
            TotalDiscovered = entity.TotalDiscovered,
            TotalCrawled = entity.TotalCrawled,
            CriticalCount = entity.CriticalCount,
            ErrorCount = entity.ErrorCount,
            WarningCount = entity.WarningCount,
            NoticeCount = entity.NoticeCount,
            Score = entity.Score,
            ScoreVersion = entity.ScoreVersion,
            HeartbeatUtc = entity.HeartbeatUtc,
            ClaimedBy = entity.ClaimedBy,
            ConfigJson = entity.ConfigJson,
            ScheduleId = entity.ScheduleId
        };

        private static SiteAuditRunEntity Map(AuditRun run) => new()
        {
            Id = run.Id,
            Key = run.Key,
            Name = run.Name,
            StatusId = (int)run.Status,
            CreatedUtc = run.CreatedUtc,
            StartedUtc = run.StartedUtc,
            FinishedUtc = run.FinishedUtc,
            StartingUrl = run.StartingUrl.ToString(),
            BaseUrl = run.BaseUrl,
            MaxPages = run.MaxPages,
            MaxDepth = run.MaxDepth,
            DelayMs = run.DelayMs,
            Concurrency = run.Concurrency,
            UserAgent = run.UserAgent,
            TotalDiscovered = run.TotalDiscovered,
            TotalCrawled = run.TotalCrawled,
            CriticalCount = run.CriticalCount,
            ErrorCount = run.ErrorCount,
            WarningCount = run.WarningCount,
            NoticeCount = run.NoticeCount,
            Score = run.Score,
            ScoreVersion = run.ScoreVersion,
            HeartbeatUtc = run.HeartbeatUtc,
            ClaimedBy = run.ClaimedBy,
            ConfigJson = run.ConfigJson,
            ScheduleId = run.ScheduleId
        };

        private static SiteAuditResourceEntity Map(int runId, AuditResource resource, IReadOnlyList<AuditIssue> issues) => new()
        {
            RunId = runId,
            UrlHash = resource.UrlHash,
            Url = Truncate(resource.Url.ToString(), 2048),
            FinalUrl = resource.FinalUrl is null ? null : Truncate(resource.FinalUrl.ToString(), 2048),
            StatusCode = resource.StatusCode,
            ContentType = Truncate(resource.ContentType, 255),
            KindId = (int)resource.Kind,
            Depth = resource.Depth,
            IsInternal = resource.IsInternal,
            ResponseTimeMs = resource.ResponseTimeMs,
            SizeBytes = resource.SizeBytes,
            RedirectCount = resource.RedirectCount,
            Title = Truncate(resource.Title, 1000),
            MetaDescription = Truncate(resource.MetaDescription, 1000),
            H1 = Truncate(resource.H1, 1000),
            WordCount = resource.WordCount,
            IndexabilityFlags = (int)resource.Indexability,
            FailureReasonId = resource.Failure.HasValue ? (int)resource.Failure.Value : null,
            UmbracoContentKey = resource.UmbracoContentKey,
            Culture = Truncate(resource.Culture, 20),
            IssueCount = issues.Count,
            ErrorCount = issues.Count(it => it.Severity >= SeoSeverity.Error),
            WarningCount = issues.Count(it => it.Severity == SeoSeverity.Warning)
        };

        private static AuditResource Map(SiteAuditResourceEntity entity) => new()
        {
            Id = entity.Id,
            RunId = entity.RunId,
            UrlHash = entity.UrlHash,
            Url = new Uri(entity.Url),
            FinalUrl = string.IsNullOrEmpty(entity.FinalUrl) ? null : new Uri(entity.FinalUrl),
            StatusCode = entity.StatusCode,
            ContentType = entity.ContentType,
            Kind = (ResourceKind)entity.KindId,
            Depth = entity.Depth,
            IsInternal = entity.IsInternal,
            ResponseTimeMs = entity.ResponseTimeMs,
            SizeBytes = entity.SizeBytes,
            RedirectCount = entity.RedirectCount,
            Title = entity.Title,
            MetaDescription = entity.MetaDescription,
            H1 = entity.H1,
            WordCount = entity.WordCount,
            Indexability = (IndexabilityFlags)entity.IndexabilityFlags,
            Failure = entity.FailureReasonId.HasValue ? (CrawlFailureReason)entity.FailureReasonId.Value : null,
            UmbracoContentKey = entity.UmbracoContentKey,
            Culture = entity.Culture,
            IssueCount = entity.IssueCount,
            ErrorCount = entity.ErrorCount,
            WarningCount = entity.WarningCount
        };

        private static SiteAuditIssueEntity Map(int runId, int? resourceId, AuditIssue issue) => new()
        {
            RunId = runId,
            ResourceId = resourceId,
            CheckAlias = Truncate(issue.CheckAlias, 255)!,
            Variant = Truncate(issue.Variant, 100),
            Severity = (int)issue.Severity,
            Url = issue.Url is null ? null : Truncate(issue.Url.ToString(), 2048),
            DataJson = issue.Data.Count == 0 ? null : JsonConvert.SerializeObject(issue.Data),
            Evidence = Truncate(issue.Evidence, 1000),
            IssueHash = issue.IssueHash
        };

        private static AuditIssue Map(SiteAuditIssueEntity entity) => new()
        {
            Id = entity.Id,
            RunId = entity.RunId,
            ResourceId = entity.ResourceId,
            CheckAlias = entity.CheckAlias,
            Variant = entity.Variant,
            Severity = (SeoSeverity)entity.Severity,
            Url = string.IsNullOrEmpty(entity.Url) ? null : new Uri(entity.Url),
            Data = string.IsNullOrEmpty(entity.DataJson)
                ? new Dictionary<string, string>(0)
                : JsonConvert.DeserializeObject<Dictionary<string, string>>(entity.DataJson)
                  ?? new Dictionary<string, string>(0),
            Evidence = entity.Evidence,
            IssueHash = entity.IssueHash
        };

        private static SiteAuditCheckRunEntity Map(int runId, AuditCheckRun checkRun) => new()
        {
            RunId = runId,
            CheckAlias = Truncate(checkRun.CheckAlias, 255)!,
            CheckName = Truncate(checkRun.CheckName, 255),
            CategoryId = (int)checkRun.Category,
            DidRun = checkRun.DidRun,
            ApplicableCount = checkRun.ApplicableCount,
            FailedCount = checkRun.FailedCount,
            IssueCount = checkRun.IssueCount,
            Weight = checkRun.Weight,
            Severity = (int)checkRun.Severity,
            DurationMs = checkRun.DurationMs
        };

        private static AuditCheckRun Map(SiteAuditCheckRunEntity entity) => new()
        {
            RunId = entity.RunId,
            CheckAlias = entity.CheckAlias,
            CheckName = entity.CheckName,
            Category = (SeoCheckCategory)entity.CategoryId,
            DidRun = entity.DidRun,
            ApplicableCount = entity.ApplicableCount,
            FailedCount = entity.FailedCount,
            IssueCount = entity.IssueCount,
            Weight = entity.Weight,
            Severity = (SeoSeverity)entity.Severity,
            DurationMs = entity.DurationMs
        };

        /// <summary>
        /// Keeps oversized values inside their column. A url or title longer than the column is a
        /// curiosity, not a reason to fail the whole batch it happens to be in.
        /// </summary>
        private static string? Truncate(string? value, int maxLength)
            => value is null || value.Length <= maxLength ? value : value[..maxLength];
    }
}
