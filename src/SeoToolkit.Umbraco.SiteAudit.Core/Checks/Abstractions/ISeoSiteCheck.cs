#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// A check that can only answer its question once it has seen the whole site - duplicate
    /// titles, orphan pages, sitemap coverage, hreflang return tags.
    /// <para>
    /// These were impossible under the old api, which only ever handed a check one page at a
    /// time and kept no shared state.
    /// </para>
    /// <para>
    /// The work happens in two phases. During the crawl each resource is offered to a collector
    /// created by <see cref="CreateCollector"/>; afterwards <see cref="FinalizeAsync"/> runs
    /// once, single threaded, with the completed index and that collector. Many checks need no
    /// collector at all because <see cref="ICrawlIndex"/> already answers the common questions.
    /// </para>
    /// </summary>
    public interface ISeoSiteCheck : ISeoCheck
    {
        /// <summary>
        /// Creates this check's per-run state, or returns null to rely solely on the crawl index.
        /// Called once per run, before the crawl starts.
        /// </summary>
        ISeoCrawlCollector? CreateCollector();

        /// <summary>
        /// Runs once after the crawl completes, with the finished index and whatever the
        /// collector gathered. Single threaded, so no synchronisation is needed here.
        /// </summary>
        ValueTask FinalizeAsync(SeoSiteCheckContext context, CancellationToken cancellationToken);
    }
}
