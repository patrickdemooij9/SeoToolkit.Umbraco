using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    public class SitemapGeneratorOptions
    {
        /// <summary>
        /// Indicates the starting node. If this is empty, it'll use all pages on the root.
        /// </summary>
        public IPublishedContent StartingNode { get; }

        /// <summary>
        /// Main culture for the finding the pages. 
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Base URL to use for generated sitemap URLs. If empty, uses the default Umbraco URL.
        /// </summary>
        public string? BaseUrl { get; }

        public SitemapGeneratorOptions(IPublishedContent startingNode, string culture, string? baseUrl = null)
        {
            StartingNode = startingNode;
            Culture = culture;
            BaseUrl = baseUrl;
        }
    }
}
