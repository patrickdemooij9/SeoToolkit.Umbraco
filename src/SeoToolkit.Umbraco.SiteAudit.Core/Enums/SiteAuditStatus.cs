namespace SeoToolkit.Umbraco.SiteAudit.Core.Enums
{
    public enum SiteAuditStatus
    {
        /// <summary>Configured but never started.</summary>
        Created = 0,
        Running = 1,
        Finished = 2,
        Error = 3,
        /// <summary>Queued, waiting for the job runner to pick it up.</summary>
        Scheduled = 4,
        /// <summary>Stopped by a person part way through. Whatever was gathered is kept.</summary>
        Stopped = 5,
        /// <summary>
        /// Started but stopped reporting - almost always an application restart part way through.
        /// Distinct from Error, which means the crawl itself failed.
        /// </summary>
        Interrupted = 6
    }
}
