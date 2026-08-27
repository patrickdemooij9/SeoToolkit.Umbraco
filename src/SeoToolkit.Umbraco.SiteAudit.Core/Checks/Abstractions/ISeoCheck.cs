#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Base contract for every check.
    /// <para>
    /// Implementations are resolved once and shared for the lifetime of the application, so
    /// they must be stateless. Anything that needs to accumulate over a crawl belongs in an
    /// <see cref="ISeoCrawlCollector"/>, which is created fresh for each run. That rule is what
    /// keeps checks free of any dependency on service lifetimes and safe to call concurrently.
    /// </para>
    /// <para>
    /// Do not implement this directly - implement <see cref="ISeoPageCheck"/> for per-resource
    /// checks or <see cref="ISeoSiteCheck"/> for checks that need the whole crawl.
    /// </para>
    /// </summary>
    public interface ISeoCheck
    {
        /// <summary>
        /// Identity, presentation, scoring weight, capabilities and options.
        /// Must return the same value every time; the crawler reads it once per run.
        /// </summary>
        SeoCheckDescriptor Descriptor { get; }
    }
}
