#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// Indexing directives, parsed once from both the robots meta tag and the X-Robots-Tag
    /// response header. Parsing this per check would mean re-reading the same string dozens
    /// of times per page.
    /// </summary>
    [Flags]
    public enum RobotsDirectives
    {
        None = 0,
        Index = 1 << 0,
        NoIndex = 1 << 1,
        Follow = 1 << 2,
        NoFollow = 1 << 3,
        NoArchive = 1 << 4,
        NoSnippet = 1 << 5,
        NoImageIndex = 1 << 6,
        NoTranslate = 1 << 7,
        None_Directive = 1 << 8
    }

    public static class RobotsDirectivesExtensions
    {
        /// <summary>
        /// The bare "none" directive is shorthand for "noindex, nofollow", so it is expanded
        /// here rather than making every caller remember that.
        /// </summary>
        public static bool BlocksIndexing(this RobotsDirectives directives)
            => (directives & (RobotsDirectives.NoIndex | RobotsDirectives.None_Directive)) != 0;

        public static bool BlocksFollowing(this RobotsDirectives directives)
            => (directives & (RobotsDirectives.NoFollow | RobotsDirectives.None_Directive)) != 0;
    }
}
