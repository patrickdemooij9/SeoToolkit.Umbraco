using System.Collections.Generic;
using System.Xml.Linq;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public interface ISitemapGenerator
    {
        /// <summary>
        /// Whether sitemaps are split into parts. Only true when MaxUrlsPerSitemap is configured.
        /// </summary>
        bool IsSplittingEnabled { get; }

        /// <summary>
        /// Generates the sitemap.xml for the given options. This is a urlset when splitting is disabled or everything fits in a single sitemap,
        /// otherwise a sitemap index referencing all parts.
        /// </summary>
        XDocument Generate(SitemapGeneratorOptions options);

        /// <summary>
        /// Generates a single part ("{alias}-{index}-sitemap.xml"). Returns null if the part does not exist or splitting is disabled.
        /// </summary>
        XDocument? GeneratePart(SitemapGeneratorOptions options, string alias, int index);

        /// <summary>
        /// Returns all parts for the given options, empty collections excluded.
        /// </summary>
        IReadOnlyList<SitemapPart> GetParts(SitemapGeneratorOptions options);
    }
}
