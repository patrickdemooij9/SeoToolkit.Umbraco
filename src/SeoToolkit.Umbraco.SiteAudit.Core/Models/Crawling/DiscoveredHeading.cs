#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// A heading, kept in document order so checks can detect skipped levels
    /// (an h4 following an h2, for example).
    /// </summary>
    public readonly struct DiscoveredHeading
    {
        public DiscoveredHeading(int level, string text)
        {
            Level = level;
            Text = text;
        }

        /// <summary>1 for h1 through 6 for h6.</summary>
        public int Level { get; }

        public string Text { get; }
    }
}
