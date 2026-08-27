#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Grouping used by the results UI and by the health score, which weights categories
    /// separately so that one noisy area cannot dominate the number.
    /// </summary>
    public enum SeoCheckCategory
    {
        Other = 0,
        /// <summary>Whether search engines can index the page at all.</summary>
        Indexability = 1,
        /// <summary>Transport, status codes, redirects, headers, url shape.</summary>
        Technical = 2,
        /// <summary>Titles, descriptions, headings, and other markup-level signals.</summary>
        OnPage = 3,
        Content = 4,
        Links = 5,
        Images = 6,
        StructuredData = 7,
        International = 8,
        Performance = 9,
        /// <summary>Checks that use knowledge of the Umbraco content tree.</summary>
        Umbraco = 10,
        Accessibility = 11
    }
}
