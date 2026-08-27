#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Convenience base for site-wide checks that work purely off the crawl index.
    /// Override <see cref="Evaluate"/> for the synchronous case.
    /// </summary>
    public abstract class SeoSiteCheckBase : ISeoSiteCheck
    {
        private SeoCheckDescriptor? _descriptor;

        public SeoCheckDescriptor Descriptor => _descriptor ??= WithIndexCapability(CreateDescriptor());

        protected abstract SeoCheckDescriptor CreateDescriptor();

        public virtual ISeoCrawlCollector? CreateCollector() => null;

        public virtual ValueTask FinalizeAsync(SeoSiteCheckContext context, CancellationToken cancellationToken)
        {
            Evaluate(context);
            return default;
        }

        /// <summary>Synchronous finalise body. Does nothing unless overridden.</summary>
        protected virtual void Evaluate(SeoSiteCheckContext context)
        {
        }

        /// <summary>
        /// A site check is always handed the index, so forgetting to declare the capability
        /// would only ever mean the crawler skipped building it. Add it rather than let that
        /// be a silent bug in every third-party check.
        /// </summary>
        private static SeoCheckDescriptor WithIndexCapability(SeoCheckDescriptor descriptor)
            => descriptor.RequiredCapabilities.Requires(SeoCheckCapabilities.CrawlIndex)
                ? descriptor
                : descriptor with
                {
                    RequiredCapabilities = descriptor.RequiredCapabilities | SeoCheckCapabilities.CrawlIndex
                };
    }

    /// <summary>
    /// Base for a site-wide check that needs to accumulate its own state during the crawl.
    /// The collector is created fresh per run and handed back at the end, so the check itself
    /// stays stateless.
    /// </summary>
    public abstract class SeoSiteCheckBase<TCollector> : SeoSiteCheckBase
        where TCollector : class, ISeoCrawlCollector, new()
    {
        public sealed override ISeoCrawlCollector? CreateCollector() => new TCollector();

        protected sealed override void Evaluate(SeoSiteCheckContext context)
        {
            if (context.Collector is TCollector collector)
                Evaluate(collector, context);
        }

        protected abstract void Evaluate(TCollector collector, SeoSiteCheckContext context);
    }
}
