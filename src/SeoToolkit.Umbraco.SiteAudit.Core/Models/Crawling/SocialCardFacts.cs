#nullable enable
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// Open Graph and Twitter card metadata. Stored as the handful of properties checks
    /// actually use plus a raw bag, rather than a full property map per page.
    /// </summary>
    public sealed class SocialCardFacts
    {
        public static readonly SocialCardFacts Empty = new SocialCardFacts();

        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? Type { get; init; }
        public Uri? Image { get; init; }
        public string? ImageAlt { get; init; }
        public Uri? Url { get; init; }
        public string? SiteName { get; init; }

        /// <summary>Twitter card type, e.g. "summary_large_image".</summary>
        public string? Card { get; init; }

        /// <summary>Any property not promoted above. Null when nothing else was present.</summary>
        public IReadOnlyDictionary<string, string>? Additional { get; init; }

        public bool IsEmpty =>
            Title is null && Description is null && Type is null && Image is null &&
            Url is null && SiteName is null && Card is null &&
            (Additional is null || Additional.Count == 0);
    }
}
