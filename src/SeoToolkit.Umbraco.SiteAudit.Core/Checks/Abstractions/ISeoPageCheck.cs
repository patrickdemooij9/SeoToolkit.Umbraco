#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// A check that looks at one resource at a time.
    /// <para>
    /// Asynchronous because some checks legitimately need to make requests of their own -
    /// verifying an external link, for instance. The old api was synchronous, which forced
    /// those checks to block on <c>.Result</c> inside a loop and tie up a thread per page.
    /// </para>
    /// <para>
    /// Returns <see cref="ValueTask"/> so the overwhelming majority of checks, which just read
    /// already-parsed facts and complete synchronously, allocate nothing at all.
    /// </para>
    /// </summary>
    public interface ISeoPageCheck : ISeoCheck
    {
        /// <summary>
        /// Runs against a single resource. Only called for resource kinds matching
        /// <see cref="SeoCheckDescriptor.AppliesTo"/>, and only when the run can supply the
        /// declared capabilities - so there is no need to re-check either.
        /// May be called concurrently for different resources.
        /// </summary>
        ValueTask RunAsync(SeoPageCheckContext context, CancellationToken cancellationToken);
    }
}
