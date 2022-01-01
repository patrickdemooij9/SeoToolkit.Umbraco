using System;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator
{
    public class SitemapIndexGenerator : ISitemapIndexGenerator
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        private XNamespace _namespace => XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");

        public SitemapIndexGenerator(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public XDocument Generate()
        {
            var rootNamespace = new XElement(_namespace + "sitemapindex");

            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                foreach (var domain in ctx.UmbracoContext.Domains.GetAll(false))
                {
                    var rootNode = ctx.UmbracoContext.Content.GetById(domain.ContentId);
                    if (rootNode is null)
                        continue;

                    var sitemapElement = new XElement(_namespace + "sitemap");

                    var baseUri = new Uri(rootNode.Url(domain.Culture, UrlMode.Absolute));
                    sitemapElement.Add(new XElement(_namespace + "loc", new Uri(baseUri, "sitemap.xml").AbsoluteUri));

                    rootNamespace.Add(sitemapElement);
                }
            }
            return new XDocument(rootNamespace);
        }
    }
}
