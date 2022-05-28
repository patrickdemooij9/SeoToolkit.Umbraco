using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public class SitemapGenerator : ISitemapGenerator
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISitemapService _sitemapService;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IEventAggregator _eventAggregator;
        private readonly SitemapConfig _settings;

        private List<string> _validAlternateCultures;
        private Dictionary<int, SitemapPageSettings> _pageTypeSettings; //Used to cache the types for the generation

        private XNamespace _namespace => XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        private XNamespace _xHtmlNamespace = XNamespace.Get("http://www.w3.org/1999/xhtml");

        public SitemapGenerator(IUmbracoContextFactory umbracoContextFactory,
            ISettingsService<SitemapConfig> settingsService,
            ISitemapService sitemapService,
            IPublicAccessService publicAccessService,
            IEventAggregator eventAggregator)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _sitemapService = sitemapService;
            _publicAccessService = publicAccessService;
            _eventAggregator = eventAggregator;
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

                    var items = GetSelfAndChildren(node, options.Culture);

                    _eventAggregator.Publish(new GenerateSitemapNotification(items));

                    rootNamespace.Add(ToXmlElements(items));

                    if (_settings.ShowAlternatePages)
                        _validAlternateCultures.Clear();
                }
            }

            return new XDocument(rootNamespace);
        }

        private List<SitemapNodeItem> GetSelfAndChildren(IPublishedContent content, string culture)
        {
            var items = new List<SitemapNodeItem>();

            //Only show item if it actually has an template, so we don't index data objects and such
            if (content.TemplateId > 0 && !_publicAccessService.IsProtected(content.Path))
            {
                var settings = GetPageTypeSettings(content.ContentType.Id);

                var item = new SitemapNodeItem(content.Url(culture, UrlMode.Absolute))
                {
                    HideFromSitemap = settings?.HideFromSitemap is true
                };

                if (settings is null || !settings.HideFromSitemap)
                {
                    if (!string.IsNullOrWhiteSpace(_settings.LastModifiedFieldAlias) && HasValue(content, _settings.LastModifiedFieldAlias, culture)) 
                        item.LastModifiedDate = Value<DateTime>(content, _settings.LastModifiedFieldAlias, culture);
                    else 
                        item.LastModifiedDate = content.UpdateDate;
                    

                    if (settings != null)
                    {
                        if (!string.IsNullOrWhiteSpace(settings.ChangeFrequency))
                            item.ChangeFrequency = settings.ChangeFrequency;

                        if (settings.Priority != null)
                            item.Priority = settings.Priority;
                    }

                    if (string.IsNullOrWhiteSpace(item.ChangeFrequency) && !string.IsNullOrWhiteSpace(_settings.ChangeFrequencyFieldAlias) && HasValue(content, _settings.ChangeFrequencyFieldAlias, culture))
                    {
                        item.ChangeFrequency = Value<string>(content, _settings.ChangeFrequencyFieldAlias, culture);
                    }

                    if (item.Priority is null && !string.IsNullOrWhiteSpace(_settings.PriorityFieldAlias) && HasValue(content, _settings.PriorityFieldAlias, culture))
                    {
                        item.Priority = Value<double?>(content, _settings.PriorityFieldAlias, culture);
                    }

                    if (_settings.ShowAlternatePages)
                    {
                        var cultures = content.Cultures.Where(it => content.IsPublished(it.Key) && _validAlternateCultures.Contains(it.Key, StringComparer.InvariantCultureIgnoreCase)).ToArray();
                        if (cultures.Length > 1)
                        {
                            foreach (var additionalCulture in cultures)
                            {
                                item.AlternatePages.Add(new SitemapNodeAlternatePage(content.Url(additionalCulture.Key, UrlMode.Absolute), additionalCulture.Key));
                            }
                        }
                    }

                    _eventAggregator.Publish(new GenerateSitemapNodeNotification(item));

                    items.Add(item);
                }
            }

            foreach (var child in content.Children(culture))
            {
                items.AddRange(GetSelfAndChildren(child, culture));
            }
            return items;
        }

        private IEnumerable<XElement> ToXmlElements(IEnumerable<SitemapNodeItem> nodes)
        {
            var items = new List<XElement>();

            foreach (var node in nodes)
            {
                if (node.HideFromSitemap) continue;

                var selfItem = new XElement(_namespace + "url");
                selfItem.Add(new XElement(_namespace + "loc", node.Url));
                if (node.LastModifiedDate != null)
                    selfItem.Add(new XElement(_namespace + "lastmod", node.LastModifiedDate.Value.ToString("yyyy-MM-dd")));
                if (!string.IsNullOrWhiteSpace(node.ChangeFrequency))
                    selfItem.Add(new XElement(_namespace + "changefreq", node.ChangeFrequency));
                if (node.Priority != null)
                    selfItem.Add(new XElement(_namespace + "priority", node.Priority));

                foreach (var alternatePage in node.AlternatePages)
                {
                    selfItem.Add(new XElement(_xHtmlNamespace + "link",
                        new XAttribute("rel", "alternate"),
                        new XAttribute("hreflang", alternatePage.Culture),
                        new XAttribute("href", alternatePage.Url)));
                }

                items.Add(selfItem);
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
