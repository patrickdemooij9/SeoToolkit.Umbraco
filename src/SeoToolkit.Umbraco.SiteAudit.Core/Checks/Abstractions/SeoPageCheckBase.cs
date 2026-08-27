#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Convenience base for per-resource checks.
    /// <para>
    /// Override <see cref="Check"/> for the normal case, which is a synchronous look at
    /// already-parsed data; that path allocates nothing. Override
    /// <see cref="RunAsync"/> instead only when the check genuinely has to await something.
    /// </para>
    /// </summary>
    public abstract class SeoPageCheckBase : ISeoPageCheck
    {
        private SeoCheckDescriptor? _descriptor;

        /// <summary>
        /// Built on first use and cached. A race here would at worst build two identical
        /// immutable descriptors, which is cheaper than locking on every access.
        /// </summary>
        public SeoCheckDescriptor Descriptor => _descriptor ??= CreateDescriptor();

        protected abstract SeoCheckDescriptor CreateDescriptor();

        public virtual ValueTask RunAsync(SeoPageCheckContext context, CancellationToken cancellationToken)
        {
            Check(in context);
            return default;
        }

        /// <summary>Synchronous check body. Does nothing unless overridden.</summary>
        protected virtual void Check(in SeoPageCheckContext context)
        {
        }
    }
}
