#nullable enable
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// One fetched resource and everything the crawler learned about it.
    /// <para>
    /// This replaces the old CrawledPageModel, which carried only a url, a status code, two
    /// timestamps and a parsed document - not enough to write most real SEO checks against.
    /// </para>
    /// <para>
    /// Instances are short lived by design: the crawler hands one to the checks, then drops it.
    /// Only a compact summary survives into the crawl index, so peak memory stays proportional
    /// to the number of in-flight requests rather than to the size of the site.
    /// </para>
    /// </summary>
    public sealed record CrawledResource
    {
        // ---- Identity and graph -------------------------------------------------------

        /// <summary>The url the crawler asked for.</summary>
        public required Uri RequestedUrl { get; init; }

        /// <summary>Where the request ended up after redirects. Equals <see cref="RequestedUrl"/> when there were none.</summary>
        public required Uri FinalUrl { get; init; }

        /// <summary>Canonical form of <see cref="RequestedUrl"/>, used for de-duplication and index lookups.</summary>
        public required string NormalizedUrl { get; init; }

        /// <summary>Stable hash of <see cref="NormalizedUrl"/>, used as the persistence key.</summary>
        public required string UrlHash { get; init; }

        /// <summary>Number of links away from the seed url. The seed itself is depth 0.</summary>
        public int Depth { get; init; }

        /// <summary>
        /// Pages that linked here. Capped by the crawler - knowing a url has thousands of
        /// inlinks is useful, storing all of them per resource is not.
        /// </summary>
        public IReadOnlyList<Uri> Referrers { get; init; } = Array.Empty<Uri>();

        public DiscoverySource DiscoveredVia { get; init; }

        // ---- Timing -------------------------------------------------------------------

        public DateTimeOffset RequestStartedUtc { get; init; }
        public DateTimeOffset RequestCompletedUtc { get; init; }
        public int TimeToFirstByteMs { get; init; }
        public int TotalMs { get; init; }

        // ---- Response -----------------------------------------------------------------

        public int StatusCode { get; init; }

        /// <summary>
        /// Response headers, lower-cased keys. Empty unless some enabled check declared
        /// <see cref="Checks.Abstractions.SeoCheckCapabilities.ResponseHeaders"/>.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> ResponseHeaders { get; init; }
            = new Dictionary<string, string[]>(0);

        public string? ContentType { get; init; }
        public string? CharSet { get; init; }

        /// <summary>Content-Length as declared by the server, when it declared one.</summary>
        public long? DeclaredContentLength { get; init; }

        /// <summary>Bytes actually received.</summary>
        public long TransferredBytes { get; init; }

        public string? ContentEncoding { get; init; }
        public string? ETag { get; init; }
        public DateTimeOffset? LastModified { get; init; }

        /// <summary>Negotiated protocol, e.g. "1.1" or "2".</summary>
        public string? Protocol { get; init; }

        // ---- Redirects ----------------------------------------------------------------

        public IReadOnlyList<RedirectHop> RedirectChain { get; init; } = Array.Empty<RedirectHop>();

        public bool IsRedirectLoop { get; init; }

        public bool WasRedirected => RedirectChain.Count > 0;

        // ---- Body ---------------------------------------------------------------------

        /// <summary>
        /// Raw response body. Only populated when an enabled check declared
        /// <see cref="Checks.Abstractions.SeoCheckCapabilities.RawBody"/> - carrying full markup
        /// for every page otherwise costs a lot of memory for no benefit.
        /// </summary>
        public string? RawBody { get; init; }

        public ResourceKind Kind { get; init; }

        /// <summary>Whether this resource is on one of the hosts the audit is scoped to.</summary>
        public bool IsInternal { get; init; }

        /// <summary>Parsed markup facts. Null for anything that is not an HTML page, or when the fetch failed.</summary>
        public PageFacts? Facts { get; init; }

        // ---- Failure ------------------------------------------------------------------

        public CrawlFailureReason? Failure { get; init; }
        public string? FailureDetail { get; init; }

        // ---- Umbraco mapping ----------------------------------------------------------

        public Guid? UmbracoContentKey { get; init; }
        public int? UmbracoContentId { get; init; }
        public string? Culture { get; init; }

        // ---- Derived ------------------------------------------------------------------

        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
        public bool IsRedirect => StatusCode >= 300 && StatusCode < 400;
        public bool IsClientError => StatusCode >= 400 && StatusCode < 500;
        public bool IsServerError => StatusCode >= 500 && StatusCode < 600;
        public bool IsFailed => Failure.HasValue && Failure.Value != CrawlFailureReason.None;
        public bool IsHtml => Kind == ResourceKind.HtmlPage;

        /// <summary>True when nothing was learned about the resource - no response and no body.</summary>
        public bool HasResponse => StatusCode > 0;

        /// <summary>
        /// Reads a response header, or null when it is absent or headers were not captured.
        /// </summary>
        public string? GetHeader(string name)
        {
            if (ResponseHeaders.Count == 0) return null;
            return ResponseHeaders.TryGetValue(name.ToLowerInvariant(), out var values) && values.Length > 0
                ? values[0]
                : null;
        }
    }
}
