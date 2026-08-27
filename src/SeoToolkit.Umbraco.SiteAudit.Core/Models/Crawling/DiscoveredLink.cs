#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>Roughly where on the page a link sits. Used to tell navigation chrome from real body links.</summary>
    public enum LinkRegion
    {
        Unknown = 0,
        Navigation = 1,
        Body = 2,
        Footer = 3,
        Header = 4,
        Aside = 5
    }

    /// <summary>A link found on a page, with the context a check needs to judge it.</summary>
    public sealed class DiscoveredLink
    {
        public DiscoveredLink(Uri url,
            string? anchorText = null,
            string? rel = null,
            bool isInternal = false,
            LinkRegion region = LinkRegion.Unknown,
            string? title = null)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            AnchorText = anchorText;
            Rel = rel;
            IsInternal = isInternal;
            Region = region;
            Title = title;
        }

        public Uri Url { get; }

        /// <summary>Visible text of the anchor, already trimmed and whitespace-collapsed.</summary>
        public string? AnchorText { get; }

        /// <summary>Raw rel attribute, if any.</summary>
        public string? Rel { get; }

        public string? Title { get; }

        public bool IsInternal { get; }

        public LinkRegion Region { get; }

        public bool IsNoFollow => HasRel("nofollow");

        public bool IsSponsored => HasRel("sponsored");

        public bool IsUgc => HasRel("ugc");

        public bool HasEmptyAnchorText => string.IsNullOrWhiteSpace(AnchorText);

        private bool HasRel(string token)
        {
            if (string.IsNullOrEmpty(Rel)) return false;

            //rel is a space separated token list; avoid Split so this stays allocation free
            //on the hot path - every link on every page runs through here.
            var span = Rel.AsSpan();
            var tokenSpan = token.AsSpan();
            var index = 0;
            while (index < span.Length)
            {
                while (index < span.Length && char.IsWhiteSpace(span[index])) index++;
                var start = index;
                while (index < span.Length && !char.IsWhiteSpace(span[index])) index++;
                if (index > start && span.Slice(start, index - start).Equals(tokenSpan, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
