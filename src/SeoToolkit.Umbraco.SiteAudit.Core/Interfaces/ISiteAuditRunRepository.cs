#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using Umbraco.Cms.Core.Models;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    /// <summary>
    /// Reads and writes audit runs and their results.
    /// <para>
    /// Every read is either a single row or explicitly paged. That is the whole point of this
    /// interface: the previous repository had no paging at all, and its Get(id) was implemented
    /// as GetAll().FirstOrDefault(), so opening one audit pulled every audit and every crawled
    /// page in the database into memory - on a hot path the backoffice polled once a second.
    /// </para>
    /// </summary>
    public interface ISiteAuditRunRepository
    {
        AuditRun? Get(int id);

        AuditRun? Get(Guid key);

        /// <summary>Runs for the overview, newest first. Never loads results.</summary>
        PagedModel<AuditRun> GetPaged(int skip, int take);

        AuditRun Save(AuditRun run);

        void Delete(int id);

        /// <summary>Removes all but the newest <paramref name="keep"/> runs.</summary>
        int DeleteAllButNewest(int keep);

        /// <summary>Runs waiting to be picked up, oldest first.</summary>
        IReadOnlyList<AuditRun> GetQueued(int max);

        /// <summary>
        /// Claims a queued run for this server. Returns false when another server got there
        /// first, which is what keeps a load balanced setup from running an audit twice.
        /// </summary>
        bool TryClaim(int runId, string claimedBy);

        void UpdateHeartbeat(int runId, int totalCrawled, int totalDiscovered);

        /// <summary>
        /// Marks runs whose heartbeat has gone stale as interrupted, so a crawl killed by an
        /// application restart does not sit at "Running" forever.
        /// </summary>
        int MarkStaleRunsInterrupted(TimeSpan staleAfter);

        /// <summary>Writes a batch of crawled resources and their findings in one go.</summary>
        void AddResults(int runId, IReadOnlyList<AuditResourceWrite> resources);

        /// <summary>Writes findings that belong to the site rather than to any one page.</summary>
        void AddSiteIssues(int runId, IReadOnlyList<AuditIssue> issues);

        void SaveCheckRuns(int runId, IReadOnlyList<AuditCheckRun> checkRuns);

        IReadOnlyList<AuditCheckRun> GetCheckRuns(int runId);

        PagedModel<AuditResource> GetResources(int runId, int skip, int take, AuditResourceFilter? filter = null);

        AuditResource? GetResource(int runId, int resourceId);

        PagedModel<AuditIssue> GetIssues(int runId, int skip, int take, AuditIssueFilter? filter = null);

        IReadOnlyList<AuditIssue> GetIssuesForResource(int runId, int resourceId);
    }

    /// <summary>A resource and the findings raised against it, written together.</summary>
    public sealed class AuditResourceWrite
    {
        public required AuditResource Resource { get; init; }
        public required IReadOnlyList<AuditIssue> Issues { get; init; }
    }
}
