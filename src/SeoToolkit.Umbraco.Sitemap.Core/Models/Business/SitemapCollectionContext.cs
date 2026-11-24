using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    public class SitemapCollectionContext
    {
        /// <summary>
        /// Root node of the sitemap. If this is empty, the sitemap covers all pages on the root (no domains or StructureMode.OnlyRoot).
        /// </summary>
        public IPublishedContent? RootNode { get; }

        /// <summary>
        /// Domain the sitemap is generated for. Empty if the website has no domains or StructureMode.OnlyRoot is used.
        /// </summary>
        public Domain? Domain { get; }

        public string? Culture { get; }

        /// <summary>
        /// Base URL to use for generated sitemap URLs. If empty, uses the default Umbraco URL.
        /// </summary>
        public string? BaseUrl { get; }

        public SitemapCollectionContext(IPublishedContent? rootNode, Domain? domain, string? culture, string? baseUrl)
        {
            RootNode = rootNode;
            Domain = domain;
            Culture = culture;
            BaseUrl = baseUrl;
        }
    }
}
