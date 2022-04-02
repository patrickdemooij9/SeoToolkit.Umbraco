using System.Xml.Linq;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator
{
    public interface ISitemapIndexGenerator
    {
        XDocument Generate();
    }
}
