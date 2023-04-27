using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Site.ExampleCode
{
    public class ExampleSitemapCollection : ISitemapCollectionProvider
    {
        public SitemapNodeItem[] GetItems()
        {
            return new SitemapNodeItem[]
            {
                new SitemapNodeItem("https://google.nl?test=true")
            };
        }
    }
}
