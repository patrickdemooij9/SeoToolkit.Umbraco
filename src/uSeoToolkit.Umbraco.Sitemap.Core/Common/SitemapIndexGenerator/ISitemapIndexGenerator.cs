using System.Xml.Linq;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator
{
    public interface ISitemapIndexGenerator
    {
        XDocument Generate();
    }
}
