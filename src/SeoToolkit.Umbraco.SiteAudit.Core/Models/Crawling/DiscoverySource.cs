#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// How the crawler first came across a url. Checks use this to tell apart, for example,
    /// a page that is only in the sitemap from one that is actually linked to.
    /// </summary>
    public enum DiscoverySource
    {
        Unknown = 0,
        /// <summary>The url the audit was started from.</summary>
        Seed = 1,
        /// <summary>Found as a link on another crawled page.</summary>
        Link = 2,
        /// <summary>Listed in a sitemap.</summary>
        Sitemap = 3,
        /// <summary>Reached by following a redirect.</summary>
        Redirect = 4,
        /// <summary>Taken from the Umbraco content tree rather than from the site itself.</summary>
        UmbracoContent = 5
    }
}
