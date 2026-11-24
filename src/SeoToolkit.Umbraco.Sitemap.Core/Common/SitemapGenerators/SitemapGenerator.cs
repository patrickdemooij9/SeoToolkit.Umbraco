using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Caching;
using SeoToolkit.Umbraco.Sitemap.Core.Collections;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators
{
    public class SitemapGenerator : ISitemapGenerator
    {
        private readonly SitemapConfig _settings;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly SitemapCollectionProviderCollection _sitemapCollectionProviders;
        private readonly AppCaches _appCaches;
        private readonly ILogger<SitemapGenerator> _logger;

        private XNamespace _namespace => XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        private XNamespace _xHtmlNamespace = XNamespace.Get("http://www.w3.org/1999/xhtml");

        public SitemapGenerator(ISettingsService<SitemapConfig> settingsService,
            IVariationContextAccessor variationContextAccessor,
            SitemapCollectionProviderCollection sitemapCollectionProviders,
            AppCaches appCaches,
            ILogger<SitemapGenerator> logger)
        {
            _variationContextAccessor = variationContextAccessor;
            _settings = settingsService.GetSettings();
            _sitemapCollectionProviders = sitemapCollectionProviders;
            _appCaches = appCaches;
            _logger = logger;
        }

        public XDocument Generate(SitemapGeneratorOptions options)
        {
            var parts = GetParts(options);
            if (parts.Count <= 1)
                return ToUrlSet(parts.SelectMany(it => it.Items));

            return ToSitemapIndex(parts);
        }

        public XDocument? GeneratePart(SitemapGeneratorOptions options, string alias, int index)
        {
            var part = GetParts(options).FirstOrDefault(it => it.Index == index && string.Equals(it.Alias, alias, StringComparison.OrdinalIgnoreCase));
            return part is null ? null : ToUrlSet(part.Items);
        }

        public IReadOnlyList<SitemapPart> GetParts(SitemapGeneratorOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Culture))
            {
                _variationContextAccessor.VariationContext = new VariationContext(options.Culture);
            }

            var context = new SitemapCollectionContext(options.StartingNode, options.Domain, options.Culture, options.BaseUrl);
            var rootUrl = GetRootUrl(options);
            var maxUrls = Math.Max(_settings.MaxUrlsPerSitemap, 1);

            var parts = new List<SitemapPart>();
            foreach (var provider in GetProviders())
            {
                if (!provider.IsEnabled(context))
                    continue;

                var items = GetItems(provider, context);
                var index = 1;
                foreach (var chunk in items.Chunk(maxUrls))
                {
                    var url = $"{rootUrl.TrimEnd('/')}/{SitemapPart.GetFileName(provider.Alias, index)}";
                    parts.Add(new SitemapPart(provider.Alias, provider.DisplayName, index, url, chunk));
                    index++;
                }
            }
            return parts;
        }

        private IEnumerable<ISitemapCollectionProvider> GetProviders()
        {
            if (_sitemapCollectionProviders is null)
                yield break;

            var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var provider in _sitemapCollectionProviders)
            {
                var alias = provider.Alias;
                if (!SitemapAliasHelper.IsValid(alias))
                {
                    _logger.LogWarning("Sitemap collection {Provider} has an invalid alias '{Alias}'. Only lowercase letters, digits and hyphens are allowed. The collection is skipped.", provider.GetType().FullName, alias);
                    continue;
                }
                if (!aliases.Add(alias))
                {
                    _logger.LogWarning("Sitemap collection {Provider} uses alias '{Alias}', which is already used by another collection. The collection is skipped.", provider.GetType().FullName, alias);
                    continue;
                }
                if (_settings.DisabledCollections?.Contains(alias, StringComparer.OrdinalIgnoreCase) == true)
                    continue;

                yield return provider;
            }
        }

        private SitemapNodeItem[] GetItems(ISitemapCollectionProvider provider, SitemapCollectionContext context)
        {
            SitemapNodeItem[] Load() => (provider.GetItems(context) ?? Enumerable.Empty<SitemapNodeItem>())
                .Where(it => it != null && !it.HideFromSitemap)
                .ToArray();

            if (_settings.CacheDurationInMinutes <= 0)
                return Load();

            var cacheKey = $"{SitemapCacheConstants.ItemsCacheKeyPrefix}{provider.Alias}_{context.RootNode?.Key.ToString() ?? "root"}_{context.Domain?.Id.ToString() ?? "none"}_{context.Culture}_{context.BaseUrl}";
            return _appCaches.RuntimeCache.GetCacheItem(cacheKey, Load, TimeSpan.FromMinutes(_settings.CacheDurationInMinutes)) ?? Array.Empty<SitemapNodeItem>();
        }

        private static string GetRootUrl(SitemapGeneratorOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.RootUrl))
                return options.RootUrl;
            if (options.StartingNode != null)
                return BaseUrlHelper.ApplyBaseUrl(options.StartingNode.Url(options.Culture, UrlMode.Absolute), options.BaseUrl);
            return "/";
        }

        private XDocument ToUrlSet(IEnumerable<SitemapNodeItem> nodes)
        {
            var rootNamespace = new XElement(_namespace + "urlset", _settings.ShowAlternatePages ? new XAttribute(XNamespace.Xmlns + "xhtml", _xHtmlNamespace) : null);
            rootNamespace.Add(ToXmlElements(nodes));
            return new XDocument(rootNamespace);
        }

        private XDocument ToSitemapIndex(IEnumerable<SitemapPart> parts)
        {
            var rootNamespace = new XElement(_namespace + "sitemapindex");
            foreach (var part in parts)
            {
                var sitemapElement = new XElement(_namespace + "sitemap", new XElement(_namespace + "loc", part.Url));
                if (part.LastModifiedDate != null)
                    sitemapElement.Add(new XElement(_namespace + "lastmod", part.LastModifiedDate.Value.ToString(_settings.LastModifiedFormat)));
                rootNamespace.Add(sitemapElement);
            }
            return new XDocument(rootNamespace);
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
                    selfItem.Add(new XElement(_namespace + "lastmod", node.LastModifiedDate.Value.ToString(_settings.LastModifiedFormat)));
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
    }
}
