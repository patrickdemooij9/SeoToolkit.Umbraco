#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>Facts about the audit as a whole, constant for its duration.</summary>
    public interface ISeoAuditRunContext
    {
        int RunId { get; }

        Uri StartingUrl { get; }

        /// <summary>
        /// The external front-end base url this audit was retargeted to, when the site is
        /// decoupled from Umbraco. Null when Umbraco serves the site itself.
        /// </summary>
        string? BaseUrl { get; }

        /// <summary>True when only one page is being checked, from the content app.</summary>
        bool IsSinglePage { get; }

        /// <summary>User agent the crawler identifies as.</summary>
        string UserAgent { get; }

        /// <summary>
        /// What the crawler can actually supply on this run. A check whose requirements are not
        /// met here is never invoked, so it does not have to defend against missing data.
        /// </summary>
        SeoCheckCapabilities AvailableCapabilities { get; }

        /// <summary>Whether a named optional feature is active for this run.</summary>
        bool IsFeatureEnabled(string feature);
    }
}
