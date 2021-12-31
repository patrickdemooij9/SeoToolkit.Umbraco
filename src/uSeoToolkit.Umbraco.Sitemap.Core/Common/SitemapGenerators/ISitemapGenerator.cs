using System.Xml.Linq;
using uSeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public interface ISitemapGenerator
    {
        XDocument Generate(SitemapGeneratorOptions options);
    }
}
