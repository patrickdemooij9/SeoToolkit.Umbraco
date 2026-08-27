#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// One step in a redirect chain. Captured because the crawler follows redirects manually -
    /// letting HttpClient follow them automatically hides exactly the information checks need.
    /// </summary>
    public sealed class RedirectHop
    {
        public RedirectHop(Uri from, Uri to, int statusCode)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            StatusCode = statusCode;
        }

        public Uri From { get; }
        public Uri To { get; }
        public int StatusCode { get; }

        public bool IsPermanent => StatusCode == 301 || StatusCode == 308;
    }
}
