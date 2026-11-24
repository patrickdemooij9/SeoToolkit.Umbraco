using System.Xml.Linq;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator
{
    public class SitemapIndexGenerator : ISitemapIndexGenerator
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISeoDomainsService _seoDomainsService;
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly SitemapConfig _settings;

        private XNamespace _namespace => XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");

        public SitemapIndexGenerator(IUmbracoContextFactory umbracoContextFactory,
            ISeoDomainsService seoDomainsService,
            ISitemapGenerator sitemapGenerator,
            ISettingsService<SitemapConfig> settingsService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _seoDomainsService = seoDomainsService;
            _sitemapGenerator = sitemapGenerator;
            _settings = settingsService.GetSettings();
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

                    if (!rootNode.IsPublished(domain.Culture))
                        continue;

                    var seoDomain = _seoDomainsService.GetByDomain(domain.Id);
                    var options = new SitemapGeneratorOptions(rootNode, domain.Culture, seoDomain?.BaseUrl) { Domain = domain };

                    // A sitemap index may not reference another sitemap index, so a domain that is split over multiple
                    // sitemaps gets all of its parts listed here instead of its own sitemap.xml.
                    var parts = _sitemapGenerator.GetParts(options);
                    if (parts.Count > 1)
                    {
                        foreach (var part in parts)
                        {
                            rootNamespace.Add(ToSitemapElement(part.Url, part));
                        }
                    }
                    else
                    {
                        var rootUrl = BaseUrlHelper.ApplyBaseUrl(rootNode.Url(domain.Culture, UrlMode.Absolute), seoDomain?.BaseUrl);
                        rootNamespace.Add(ToSitemapElement($"{rootUrl.TrimEnd('/')}/sitemap.xml", parts.Count == 1 ? parts[0] : null));
                    }
                }
            }
            return new XDocument(rootNamespace);
        }

        private XElement ToSitemapElement(string url, SitemapPart? part)
        {
            var sitemapElement = new XElement(_namespace + "sitemap");
            sitemapElement.Add(new XElement(_namespace + "loc", url));
            if (part?.LastModifiedDate != null)
                sitemapElement.Add(new XElement(_namespace + "lastmod", part.LastModifiedDate.Value.ToString(_settings.LastModifiedFormat)));
            return sitemapElement;
        }
    }
}
