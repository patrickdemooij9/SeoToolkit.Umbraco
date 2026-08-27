#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>A single hreflang annotation.</summary>
    public sealed class HreflangEntry
    {
        public HreflangEntry(string language, Uri url)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        /// <summary>Raw hreflang value, e.g. "en-gb" or "x-default".</summary>
        public string Language { get; }

        public Uri Url { get; }

        public bool IsXDefault => string.Equals(Language, "x-default", StringComparison.OrdinalIgnoreCase);
    }
}
