#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// What a crawled resource turned out to be, based on its content type.
    /// </summary>
    public enum ResourceKind
    {
        Unknown = 0,
        HtmlPage = 1,
        Image = 2,
        Script = 3,
        Stylesheet = 4,
        Document = 5,
        Media = 6,
        Other = 7
    }

    /// <summary>
    /// Flags version of <see cref="ResourceKind"/>, used by checks to declare what they apply to.
    /// The crawler uses this to skip a check entirely rather than calling it and having it
    /// immediately return - with dozens of checks and thousands of resources, that adds up.
    /// </summary>
    [Flags]
    public enum ResourceKinds
    {
        None = 0,
        HtmlPage = 1 << ResourceKind.HtmlPage,
        Image = 1 << ResourceKind.Image,
        Script = 1 << ResourceKind.Script,
        Stylesheet = 1 << ResourceKind.Stylesheet,
        Document = 1 << ResourceKind.Document,
        Media = 1 << ResourceKind.Media,
        Other = 1 << ResourceKind.Other,
        Unknown = 1 << ResourceKind.Unknown,

        All = HtmlPage | Image | Script | Stylesheet | Document | Media | Other | Unknown
    }

    public static class ResourceKindExtensions
    {
        /// <summary>Converts a single kind into its flag, so it can be tested against a mask.</summary>
        public static ResourceKinds ToFlag(this ResourceKind kind) => (ResourceKinds)(1 << (int)kind);

        public static bool Matches(this ResourceKinds mask, ResourceKind kind) => (mask & kind.ToFlag()) != 0;
    }
}
