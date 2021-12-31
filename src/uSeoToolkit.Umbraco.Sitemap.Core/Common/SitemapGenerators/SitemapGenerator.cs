using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public class SitemapGenerator : ISitemapGenerator
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private XNamespace _namespace => XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        private XNamespace _xHtmlNamespace = XNamespace.Get("http://www.w3.org/1999/xhtml");

        public SitemapGenerator(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public XDocument Generate(SitemapGeneratorOptions options)
        {
            var rootNamespace = new XElement(_namespace + "urlset", options.IncludeAlternatePages ? new XAttribute(XNamespace.Xmlns + "xhtml", _xHtmlNamespace) : null);

            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var startingNodes = new List<IPublishedContent>();
                if (options.StartingNode != null)
                    startingNodes.Add(options.StartingNode);
                else
                    startingNodes.AddRange(ctx.UmbracoContext.Content.GetAtRoot(options.Culture));

                foreach (var node in startingNodes)
                {
                    rootNamespace.Add(GetSelfAndChildren(node, options.Culture, options.IncludeAlternatePages));
                }
            }

            return new XDocument(rootNamespace);
        }

        private IEnumerable<XElement> GetSelfAndChildren(IPublishedContent content, string culture, bool showAlternatePages)
        {
            var items = new List<XElement>();

            if (content.TemplateId > 0)
            {
                //Only show item if it actually has an template, so we don't index data objects and such
                var selfItem = new XElement(_namespace + "url");
                selfItem.Add(new XElement(_namespace + "loc", content.Url(culture, UrlMode.Absolute)));
                if (showAlternatePages)
                {
                    //TODO: We need to check if the domain exists, because now it just returns the english domain
                    var cultures = content.Cultures.Where(it => content.IsPublished(it.Key)).ToArray();
                    if (cultures.Length > 1)
                    {
                        foreach (var additionalCulture in cultures)
                        {
                            selfItem.Add(new XElement(_xHtmlNamespace + "link",
                                new XAttribute("rel", "alternate"),
                                new XAttribute("hreflang", additionalCulture.Key),
                                new XAttribute("href", content.Url(additionalCulture.Key))));
                        }
                    }
                }
                items.Add(selfItem);
            }

            foreach (var child in content.Children(culture))
            {
                items.AddRange(GetSelfAndChildren(child, culture, showAlternatePages));
            }
            return items;
        }
    }
}
