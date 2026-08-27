#nullable enable
using System;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// What a check is given for one resource.
    /// <para>
    /// A readonly struct rather than a class: one of these exists per resource per check, so on
    /// a ten thousand page crawl with forty checks that would otherwise be four hundred thousand
    /// allocations doing nothing but pointing at data that already exists.
    /// </para>
    /// <para>
    /// Synchronous entry points take it by <c>in</c> to avoid even the copy;
    /// <see cref="ISeoPageCheck.RunAsync"/> cannot, because a method with an <c>in</c> parameter
    /// may not be <c>async</c>. Copying six references is still far cheaper than a heap
    /// allocation, so that path loses nothing worth having.
    /// </para>
    /// </summary>
    public readonly struct SeoPageCheckContext
    {
        private readonly ISeoIssueSink _sink;
        private readonly SeoCheckDescriptor _descriptor;

        public SeoPageCheckContext(CrawledResource resource,
            SeoCheckDescriptor descriptor,
            SeoCheckOptions options,
            ISeoAuditRunContext run,
            ISeoIssueSink sink,
            ICrawlIndex? index = null)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Options = options ?? SeoCheckOptions.Empty;
            Run = run ?? throw new ArgumentNullException(nameof(run));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            Index = index;
        }

        public CrawledResource Resource { get; }

        /// <summary>Parsed markup, or null when this resource is not an HTML page.</summary>
        public PageFacts? Facts => Resource.Facts;

        /// <summary>This check's options, already resolved for the run.</summary>
        public SeoCheckOptions Options { get; }

        public ISeoAuditRunContext Run { get; }

        /// <summary>
        /// The site-wide index, or null when this check did not declare
        /// <see cref="SeoCheckCapabilities.CrawlIndex"/>.
        /// </summary>
        public ICrawlIndex? Index { get; }

        /// <summary>
        /// Records a problem with this resource and returns a handle for adding detail.
        /// The issue counts from the moment this is called, so the return value can be ignored.
        /// </summary>
        public SeoCheckIssueBuilder Report(string? variant = null, SeoSeverity? severity = null)
        {
            var issue = new SeoCheckIssue(
                _descriptor.Alias,
                severity ?? _descriptor.DefaultSeverity,
                variant,
                Resource.RequestedUrl);

            _sink.Add(issue);
            return new SeoCheckIssueBuilder(issue);
        }
    }
}
