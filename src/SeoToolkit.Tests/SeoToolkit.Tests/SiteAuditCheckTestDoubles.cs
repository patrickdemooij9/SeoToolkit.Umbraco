using System.Collections.Concurrent;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    /// <summary>Collects reported issues so a test can assert on them.</summary>
    internal sealed class RecordingIssueSink : ISeoIssueSink
    {
        private readonly ConcurrentQueue<SeoCheckIssue> _issues = new();

        public void Add(SeoCheckIssue issue) => _issues.Enqueue(issue);

        public IReadOnlyList<SeoCheckIssue> Issues => _issues.ToArray();

        public SeoCheckIssue? Single() => _issues.Count == 1 ? _issues.First() : null;
    }

    internal sealed class FakeRunContext : ISeoAuditRunContext
    {
        public int RunId { get; init; } = 1;
        public Uri StartingUrl { get; init; } = new("https://example.com/");
        public string? BaseUrl { get; init; }
        public bool IsSinglePage { get; init; }
        public string UserAgent { get; init; } = "SeoToolkit-Tests";
        public SeoCheckCapabilities AvailableCapabilities { get; init; } = SeoCheckCapabilities.CrawlIndex;
        public HashSet<string> Features { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        public bool IsFeatureEnabled(string feature) => Features.Contains(feature);
    }

    /// <summary>
    /// Builds a <see cref="CrawledResource"/> with sensible defaults so each test only has to
    /// state the one thing it cares about.
    /// </summary>
    internal sealed class CrawledResourceBuilder
    {
        private Uri _url = new("https://example.com/page");
        private int _statusCode = 200;
        private ResourceKind _kind = ResourceKind.HtmlPage;
        private bool _isInternal = true;
        private int _depth;
        private PageFacts? _facts;
        private IReadOnlyDictionary<string, string[]>? _headers;
        private IReadOnlyList<RedirectHop>? _redirects;

        public static CrawledResourceBuilder A() => new();

        public CrawledResourceBuilder Url(string url) { _url = new Uri(url); return this; }
        public CrawledResourceBuilder Status(int statusCode) { _statusCode = statusCode; return this; }
        public CrawledResourceBuilder Kind(ResourceKind kind) { _kind = kind; return this; }
        public CrawledResourceBuilder External() { _isInternal = false; return this; }
        public CrawledResourceBuilder Depth(int depth) { _depth = depth; return this; }
        public CrawledResourceBuilder Facts(PageFacts facts) { _facts = facts; return this; }
        public CrawledResourceBuilder NoFacts() { _facts = null; return this; }

        public CrawledResourceBuilder Title(string? title)
        {
            _facts = new PageFacts { Title = title, TitleCount = title is null ? 0 : 1 };
            return this;
        }

        public CrawledResourceBuilder Headers(params (string Name, string Value)[] headers)
        {
            _headers = headers.ToDictionary(it => it.Name.ToLowerInvariant(), it => new[] { it.Value });
            return this;
        }

        public CrawledResourceBuilder Redirects(params RedirectHop[] hops)
        {
            _redirects = hops;
            return this;
        }

        public CrawledResource Build() => new()
        {
            RequestedUrl = _url,
            FinalUrl = _url,
            NormalizedUrl = _url.AbsoluteUri,
            UrlHash = _url.AbsoluteUri.GetHashCode().ToString("x8"),
            StatusCode = _statusCode,
            Kind = _kind,
            IsInternal = _isInternal,
            Depth = _depth,
            Facts = _facts,
            ResponseHeaders = _headers ?? new Dictionary<string, string[]>(0),
            RedirectChain = _redirects ?? Array.Empty<RedirectHop>()
        };
    }

    internal static class CheckTestExtensions
    {
        /// <summary>Runs a page check and returns whatever it reported.</summary>
        public static IReadOnlyList<SeoCheckIssue> Run(this ISeoPageCheck check,
            CrawledResource resource,
            ICrawlIndex? index = null,
            IReadOnlyDictionary<string, object>? optionOverrides = null,
            ISeoAuditRunContext? run = null)
        {
            var sink = new RecordingIssueSink();
            var options = SeoCheckOptionsResolver.Resolve(check.Descriptor, optionOverrides);
            var context = new SeoPageCheckContext(
                resource,
                check.Descriptor,
                options,
                run ?? new FakeRunContext(),
                sink,
                index);

            check.RunAsync(context, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            return sink.Issues;
        }

        /// <summary>
        /// Drives a site check end to end: creates its collector, feeds it every resource,
        /// then finalises. Mirrors what the crawler will do.
        /// </summary>
        public static IReadOnlyList<SeoCheckIssue> RunOver(this ISeoSiteCheck check,
            ICrawlIndex index,
            IEnumerable<CrawledResource> resources,
            IReadOnlyDictionary<string, object>? optionOverrides = null)
        {
            var sink = new RecordingIssueSink();
            var options = SeoCheckOptionsResolver.Resolve(check.Descriptor, optionOverrides);
            var run = new FakeRunContext();
            var collector = check.CreateCollector();

            if (collector is not null)
            {
                foreach (var resource in resources)
                {
                    var pageContext = new SeoPageCheckContext(
                        resource, check.Descriptor, options, run, sink, index);
                    collector.Observe(in pageContext);
                }
            }

            var siteContext = new SeoSiteCheckContext(
                check.Descriptor, options, run, sink, index, collector);

            check.FinalizeAsync(siteContext, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            return sink.Issues;
        }
    }
}
