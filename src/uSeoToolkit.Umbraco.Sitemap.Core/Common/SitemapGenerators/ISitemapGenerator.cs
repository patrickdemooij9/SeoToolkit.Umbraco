using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public interface ISitemapGenerator
    {
        XDocument Generate(string culture);
        XDocument Generate(IPublishedContent startingNode, string culture);
    }
}
