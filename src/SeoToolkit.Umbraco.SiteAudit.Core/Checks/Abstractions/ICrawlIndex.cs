#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>Which field a duplicate grouping is over.</summary>
    public enum DuplicateFacet
    {
        Title = 0,
        MetaDescription = 1,
        H1 = 2,
        Content = 3
    }

    /// <summary>
    /// The site-wide view the crawler maintains while it runs.
    /// <para>
    /// It exists so that the common site-wide questions - what links here, which pages share a
    /// title, which sitemap urls were never reached - are answered once by the crawler instead
    /// of each check building its own copy of the same lookup. Four separate duplicate checks
    /// sharing one grouping pass is the difference between linear and quadratic work.
    /// </para>
    /// <para>
    /// Only available to checks that declared <see cref="SeoCheckCapabilities.CrawlIndex"/>.
    /// Reads are safe from multiple threads; writes are the crawler's business.
    /// </para>
    /// </summary>
    public interface ICrawlIndex
    {
        /// <summary>Number of resources recorded so far.</summary>
        int Count { get; }

        bool TryGet(string normalizedUrl, [MaybeNullWhen(false)] out ResourceSummary summary);

        bool TryGetById(int id, [MaybeNullWhen(false)] out ResourceSummary summary);

        /// <summary>
        /// Every recorded resource. Streamed rather than materialised so a site check can scan
        /// a large crawl without doubling its memory.
        /// </summary>
        IEnumerable<ResourceSummary> All { get; }

        /// <summary>Ids of resources linking to the given resource.</summary>
        IReadOnlyList<int> InlinksTo(int resourceId);

        /// <summary>Ids of resources the given resource links to.</summary>
        IReadOnlyList<int> OutlinksFrom(int resourceId);

        /// <summary>
        /// Groups of resources sharing the same value for a facet. Only groups with more than
        /// one member are returned.
        /// </summary>
        IEnumerable<IReadOnlyList<ResourceSummary>> DuplicatesBy(DuplicateFacet facet);

        /// <summary>Normalised urls declared in the site's sitemaps, whether or not they were reached.</summary>
        IReadOnlyCollection<string> SitemapUrls { get; }

        /// <summary>Normalised urls derived from the Umbraco content tree, for orphan detection.</summary>
        IReadOnlyCollection<string> UmbracoUrls { get; }

        /// <summary>Whether robots.txt allows the crawler's user agent to fetch a url.</summary>
        bool IsAllowedByRobots(Uri url);
    }
}
