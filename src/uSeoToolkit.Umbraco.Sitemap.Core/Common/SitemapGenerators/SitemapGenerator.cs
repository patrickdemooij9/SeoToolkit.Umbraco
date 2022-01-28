using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using uSeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using uSeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public class SitemapGenerator : ISitemapGenerator
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISitemapService _sitemapService;
        private readonly SitemapConfig _settings;

        private List<string> _validAlternateCultures;
        private Dictionary<int, SitemapPageSettings> _pageTypeSettings; //Used to cache the types for the generation

        private XNamespace _namespace => XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        private XNamespace _xHtmlNamespace = XNamespace.Get("http://www.w3.org/1999/xhtml");

        public SitemapGenerator(IUmbracoContextFactory umbracoContextFactory,
            ISettingsService<SitemapConfig> settingsService,
            ISitemapService sitemapService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _sitemapService = sitemapService;
            _settings = settingsService.GetSettings();

            _pageTypeSettings = new Dictionary<int, SitemapPageSettings>();
        }

        public XDocument Generate(SitemapGeneratorOptions options)
        {
            _validAlternateCultures = new List<string>();
            var rootNamespace = new XElement(_namespace + "urlset", _settings.ShowAlternatePages ? new XAttribute(XNamespace.Xmlns + "xhtml", _xHtmlNamespace) : null);

            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var startingNodes = new List<IPublishedContent>();
                if (options.StartingNode != null)
                    startingNodes.Add(options.StartingNode);
                else
                    startingNodes.AddRange(ctx.UmbracoContext.Content.GetAtRoot(options.Culture));

                foreach (var node in startingNodes)
                {
                    //We need to check the domains to figure out what cultures are valid for the alternate pages. Umbraco will otherwise just return the english variant for whatever reason
                    if (_settings.ShowAlternatePages)
                    {
                        _validAlternateCultures.AddRange(ctx.UmbracoContext.Domains.GetAssigned(node.Root().Id).Select(it => it.Culture));
                    }
                    rootNamespace.Add(GetSelfAndChildren(node, options.Culture));
                    if (_settings.ShowAlternatePages)
                        _validAlternateCultures.Clear();
                }
            }

            return new XDocument(rootNamespace);
        }

        private IEnumerable<XElement> GetSelfAndChildren(IPublishedContent content, string culture)
        {
            var items = new List<XElement>();

            //Only show item if it actually has an template, so we don't index data objects and such
            if (content.TemplateId > 0)
            {
                var settings = GetPageTypeSettings(content.ContentType.Id);
                if (settings is null || !settings.HideFromSitemap)
                {
                    var hasChangeFrequency = false;
                    var hasPriority = false;

                    var selfItem = new XElement(_namespace + "url");
                    selfItem.Add(new XElement(_namespace + "loc", content.Url(culture, UrlMode.Absolute)));

                    if (!string.IsNullOrWhiteSpace(_settings.LastModifiedFieldAlias) && HasValue(content, _settings.LastModifiedFieldAlias, culture))
                    {
                        selfItem.Add(new XElement(_namespace + "lastmod", Value<DateTime>(content, _settings.LastModifiedFieldAlias, culture).ToString("yyyy-MM-dd")));
                    }
                    else
                    {
                        selfItem.Add(new XElement(_namespace + "lastmod", content.UpdateDate.ToString("yyyy-MM-dd")));
                    }

                    if (settings != null)
                    {
                        if (!string.IsNullOrWhiteSpace(settings.ChangeFrequency))
                        {
                            selfItem.Add(new XElement(_namespace + "changefreq", settings.ChangeFrequency));
                            hasChangeFrequency = true;
                        }

                        if (settings.Priority != null)
                        {
                            selfItem.Add(new XElement(_namespace + "priority", settings.Priority));
                            hasPriority = true;
                        }
                    }

                    if (!hasChangeFrequency && !string.IsNullOrWhiteSpace(_settings.ChangeFrequencyFieldAlias) && HasValue(content, _settings.ChangeFrequencyFieldAlias, culture))
                    {
                        selfItem.Add(new XElement(_namespace + "changefreq", Value<string>(content, _settings.ChangeFrequencyFieldAlias, culture)));
                    }

                    if (!hasPriority && !string.IsNullOrWhiteSpace(_settings.PriorityFieldAlias) && HasValue(content, _settings.PriorityFieldAlias, culture))
                    {
                        selfItem.Add(new XElement(_namespace + "priority", Value<decimal>(content, _settings.PriorityFieldAlias, culture)));
                    }

                    if (_settings.ShowAlternatePages)
                    {
                        var cultures = content.Cultures.Where(it => content.IsPublished(it.Key) && _validAlternateCultures.Contains(it.Key, StringComparer.InvariantCultureIgnoreCase)).ToArray();
                        if (cultures.Length > 1)
                        {
                            foreach (var additionalCulture in cultures)
                            {
                                selfItem.Add(new XElement(_xHtmlNamespace + "link",
                                    new XAttribute("rel", "alternate"),
                                    new XAttribute("hreflang", additionalCulture.Key),
                                    new XAttribute("href", content.Url(additionalCulture.Key, UrlMode.Absolute))));
                            }
                        }
                    }

                    items.Add(selfItem);
                }
            }

            foreach (var child in content.Children(culture))
            {
                items.AddRange(GetSelfAndChildren(child, culture));
            }
            return items;
        }

        //Sadly this is a bug with Umbraco itself: https://github.com/umbraco/Umbraco-CMS/issues/11815. We need to check with culture first and then without culture
        private bool HasValue(IPublishedContent content, string alias, string culture)
        {
            var hasValue = content.HasValue(alias, culture);
            if (hasValue) return hasValue;
            return content.HasValue(alias);
        }

        //Sadly this is a bug with Umbraco itself: https://github.com/umbraco/Umbraco-CMS/issues/11815. We need to check with culture first and then without culture
        private T Value<T>(IPublishedContent content, string alias, string culture)
        {
            if (content.HasValue(alias, culture))
                return content.Value<T>(alias, culture);
            if (content.HasValue(alias))
                return content.Value<T>(alias);
            return default;
        }

        private SitemapPageSettings GetPageTypeSettings(int contentTypeId)
        {
            if (!_pageTypeSettings.ContainsKey(contentTypeId))
                _pageTypeSettings[contentTypeId] = _sitemapService.GetPageTypeSettings(contentTypeId);
            return _pageTypeSettings[contentTypeId];
        }
    }
}
