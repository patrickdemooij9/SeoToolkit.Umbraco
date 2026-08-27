#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Why a resource can or cannot be indexed, worked out once during the crawl so that
    /// site-wide checks do not each have to re-derive it from raw facts.
    /// </summary>
    [Flags]
    public enum IndexabilityFlags
    {
        None = 0,
        /// <summary>Nothing is stopping this page from being indexed.</summary>
        Indexable = 1 << 0,
        NoIndex = 1 << 1,
        BlockedByRobotsTxt = 1 << 2,
        /// <summary>Canonical points somewhere else, so this url is not the one that will rank.</summary>
        CanonicalisedAway = 1 << 3,
        Redirected = 1 << 4,
        /// <summary>Returned a 4xx or 5xx.</summary>
        ErrorStatus = 1 << 5,
        /// <summary>Could not be fetched at all.</summary>
        Unreachable = 1 << 6,
        /// <summary>Not on a host the audit is scoped to.</summary>
        External = 1 << 7
    }
}
