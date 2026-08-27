#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// A script[type=application/ld+json] block. The raw text is kept rather than a parsed
    /// object graph: only a couple of checks care, and holding parsed json for every page
    /// of a large crawl is not worth the memory.
    /// </summary>
    public sealed class JsonLdBlock
    {
        public JsonLdBlock(string raw, string? type = null, string? parseError = null)
        {
            Raw = raw;
            Type = type;
            ParseError = parseError;
        }

        public string Raw { get; }

        /// <summary>The @type value, when it could be read cheaply.</summary>
        public string? Type { get; }

        /// <summary>Set when the block is not valid json.</summary>
        public string? ParseError { get; }

        public bool IsValid => ParseError is null;
    }
}
