using Umbraco.Cms.Core.Models.PublishedContent;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Models.Business
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

        public SitemapGeneratorOptions(IPublishedContent startingNode, string culture)
        {
            StartingNode = startingNode;
            Culture = culture;
        }
    }
}
