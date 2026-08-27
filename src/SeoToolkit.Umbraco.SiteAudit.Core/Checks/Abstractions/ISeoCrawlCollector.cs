#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Per-run state for a site-wide check.
    /// <para>
    /// Checks themselves are stateless singletons, which is what keeps them safe to share and
    /// free of any dependency on service lifetimes. Anything a check needs to accumulate across
    /// a crawl lives here instead, in an object created fresh for each run.
    /// </para>
    /// <para>
    /// <see cref="Observe"/> is called from the crawler's worker threads and may run
    /// concurrently for different resources, so implementations must be thread safe.
    /// Prefer accumulating into concurrent collections or interlocked counters and doing the
    /// real work in the check's finalise step, which runs single threaded.
    /// </para>
    /// </summary>
    public interface ISeoCrawlCollector
    {
        /// <summary>
        /// Called once per crawled resource. Keep this cheap and thread safe - it sits directly
        /// in the crawl loop.
        /// </summary>
        void Observe(in SeoPageCheckContext context);
    }
}
