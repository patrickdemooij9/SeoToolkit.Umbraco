using System.Xml.Linq;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public interface ISitemapGenerator
    {
        XDocument Generate(SitemapGeneratorOptions options);
    }
}
