#nullable enable
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    /// <summary>How a trailing slash is treated when comparing two urls.</summary>
    public enum TrailingSlashPolicy
    {
        /// <summary>Keep whatever the url had. Safest, but treats /about and /about/ as two pages.</summary>
        Preserve = 0,
        /// <summary>Strip it, so /about/ and /about are the same page. Sensible for most Umbraco sites.</summary>
        Remove = 1,
        /// <summary>Always add one to paths that have no file extension.</summary>
        Add = 2
    }

    /// <summary>
    /// Everything the crawler needs to know about how to behave on a run.
    /// Defaults are deliberately conservative: auditing your own production site should never
    /// look like an attack on it.
    /// </summary>
    public sealed class CrawlOptions
    {
        /// <summary>Query parameters dropped during normalisation because they never identify a distinct page.</summary>
        public static readonly string[] DefaultStrippedQueryParameters =
        {
            "utm_*", "gclid", "gbraid", "wbraid", "fbclid", "msclkid", "mc_cid", "mc_eid", "_ga", "ref"
        };

        public int? MaxPages { get; init; }

        /// <summary>How many links away from the seed to go. Null means no limit.</summary>
        public int? MaxDepth { get; init; }

        /// <summary>
        /// Minimum gap between requests to the same host. Applied per host rather than globally,
        /// so checking external links cannot starve the crawl of the site being audited.
        /// </summary>
        public int DelayBetweenRequestsMs { get; init; }

        /// <summary>
        /// How many requests may be in flight at once. Five is a deliberately polite default -
        /// this is usually pointed at the customer's own live site.
        /// </summary>
        public int MaxConcurrency { get; init; } = 5;

        public string UserAgent { get; init; } = "SeoToolkit-SiteAudit";

        /// <summary>
        /// Whether to obey robots.txt. On by default, but genuinely worth turning off sometimes:
        /// an auditor often needs to see precisely the pages that are being blocked.
        /// </summary>
        public bool RespectRobotsTxt { get; init; } = true;

        /// <summary>Seed the crawl from the site's sitemaps as well as from the starting url.</summary>
        public bool SeedFromSitemap { get; init; } = true;

        public bool AllowInvalidCertificates { get; init; }

        public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>Responses larger than this are abandoned rather than read into memory.</summary>
        public long MaxResponseBytes { get; init; } = 10 * 1024 * 1024;

        public int MaxRedirects { get; init; } = 5;

        /// <summary>
        /// Hosts considered part of the site. Anything else is external: recorded if linked to,
        /// but never crawled for more links.
        /// </summary>
        public IReadOnlyCollection<string> ScopeHosts { get; init; } = Array.Empty<string>();

        public TrailingSlashPolicy TrailingSlash { get; init; } = TrailingSlashPolicy.Remove;

        /// <summary>Query parameters removed during normalisation. Entries may end in * to match a prefix.</summary>
        public IReadOnlyCollection<string> StripQueryParameters { get; init; } = DefaultStrippedQueryParameters;

        /// <summary>
        /// Sort remaining query parameters so ?a=1&amp;b=2 and ?b=2&amp;a=1 are recognised as one page.
        /// </summary>
        public bool SortQueryParameters { get; init; } = true;

        /// <summary>
        /// Verify that linked images, scripts and stylesheets resolve. Off by default because it
        /// can easily double the number of requests a crawl makes.
        /// </summary>
        public bool CrawlAssets { get; init; }

        /// <summary>
        /// How many referrers to remember per resource. Knowing a url has thousands of inlinks is
        /// useful; storing all of them on every resource is not.
        /// </summary>
        public int MaxReferrersPerResource { get; init; } = 5;

        /// <summary>Optional per-host credentials, for auditing a frontend behind basic auth.</summary>
        public IReadOnlyDictionary<string, CrawlAuthentication> Authentication { get; init; }
            = new Dictionary<string, CrawlAuthentication>(0);
    }

    public enum CrawlAuthenticationType
    {
        None = 0,
        Basic = 1,
        Bearer = 2,
        Header = 3
    }

    /// <summary>
    /// Credentials for one host. Sourced from configuration rather than the database - storing
    /// crawl credentials in a table is a liability worth not creating.
    /// </summary>
    public sealed class CrawlAuthentication
    {
        public CrawlAuthenticationType Type { get; init; }
        public string? UserName { get; init; }
        public string? Password { get; init; }
        public string? Token { get; init; }
        public string? HeaderName { get; init; }
    }
}
