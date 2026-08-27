#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// What a site-wide check is given once the crawl has finished.
    /// <para>
    /// A class rather than a struct, because unlike the per-resource context this is created
    /// once per check per run - the allocation is irrelevant and the extra clarity is not.
    /// </para>
    /// </summary>
    public sealed class SeoSiteCheckContext
    {
        private readonly ISeoIssueSink _sink;
        private readonly SeoCheckDescriptor _descriptor;

        public SeoSiteCheckContext(SeoCheckDescriptor descriptor,
            SeoCheckOptions options,
            ISeoAuditRunContext run,
            ISeoIssueSink sink,
            ICrawlIndex index,
            ISeoCrawlCollector? collector = null)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Options = options ?? SeoCheckOptions.Empty;
            Run = run ?? throw new ArgumentNullException(nameof(run));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            Index = index ?? throw new ArgumentNullException(nameof(index));
            Collector = collector;
        }

        public SeoCheckOptions Options { get; }

        public ISeoAuditRunContext Run { get; }

        /// <summary>The completed crawl index.</summary>
        public ICrawlIndex Index { get; }

        /// <summary>
        /// The state this check accumulated during the crawl, or null if it did not ask for any.
        /// </summary>
        public ISeoCrawlCollector? Collector { get; }

        /// <summary>
        /// Records a problem against a specific url.
        /// </summary>
        public SeoCheckIssueBuilder Report(Uri url, string? variant = null, SeoSeverity? severity = null)
            => CreateIssue(url, variant, severity);

        /// <summary>
        /// Records a problem with the site as a whole rather than any one page.
        /// </summary>
        public SeoCheckIssueBuilder ReportSite(string? variant = null, SeoSeverity? severity = null)
            => CreateIssue(null, variant, severity);

        private SeoCheckIssueBuilder CreateIssue(Uri? url, string? variant, SeoSeverity? severity)
        {
            var issue = new SeoCheckIssue(
                _descriptor.Alias,
                severity ?? _descriptor.DefaultSeverity,
                variant,
                url);

            _sink.Add(issue);
            return new SeoCheckIssueBuilder(issue);
        }
    }
}
