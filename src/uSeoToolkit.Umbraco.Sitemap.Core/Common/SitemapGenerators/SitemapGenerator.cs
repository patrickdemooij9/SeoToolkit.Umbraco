using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public class SitemapGenerator : ISitemapGenerator
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private XNamespace _namespace => XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");

        public SitemapGenerator(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public XDocument Generate(string culture)
        {
            var rootNamespace = new XElement(_namespace + "urls");

            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                foreach (var rootItem in ctx.UmbracoContext.Content.GetAtRoot(culture))
                {
                    rootNamespace.Add(GetSelfAndChildren(rootItem, culture));
                }
            }

            return new XDocument(rootNamespace);
        }

        public XDocument Generate(IPublishedContent startingNode, string culture)
        {
            var rootNamespace = new XElement(_namespace + "urls");
            rootNamespace.Add(GetSelfAndChildren(startingNode, culture));
            return new XDocument(rootNamespace);
        }

        private IEnumerable<XElement> GetSelfAndChildren(IPublishedContent content, string culture)
        {
            var selfItem = new XElement(_namespace + "url");
            selfItem.Add(new XElement(_namespace + "loc", content.Url(culture, UrlMode.Absolute)));
            var items = new List<XElement> {selfItem};
            foreach (var child in content.Children(culture))
            {
                items.AddRange(GetSelfAndChildren(child, culture));
            }
            return items;
        }
    }
}
