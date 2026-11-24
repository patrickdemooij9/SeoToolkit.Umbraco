using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.CollectionProviders
{
    /// <summary>
    /// Default collection containing all Umbraco content pages.
    /// </summary>
    public class ContentSitemapCollectionProvider : ISitemapCollectionProvider
    {
        public const string ProviderAlias = "pages";

        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISitemapService _sitemapService;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsService<SitemapConfig> _settingsService;
        private readonly IDocumentNavigationQueryService _documentNavigationQueryService;

        public string Alias => ProviderAlias;
        public string DisplayName => "Pages";

        public ContentSitemapCollectionProvider(IUmbracoContextFactory umbracoContextFactory,
            ISitemapService sitemapService,
            IPublicAccessService publicAccessService,
            IEventAggregator eventAggregator,
            ISettingsService<SitemapConfig> settingsService,
            IDocumentNavigationQueryService documentNavigationQueryService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _sitemapService = sitemapService;
            _publicAccessService = publicAccessService;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;
            _documentNavigationQueryService = documentNavigationQueryService;
        }

        public IEnumerable<SitemapNodeItem> GetItems(SitemapCollectionContext context)
        {
            var generation = new Generation(_settingsService.GetSettings(), context.BaseUrl);

            // Pre-load all content overrides once per generation to avoid N+1 queries
            foreach (var contentSetting in _sitemapService.GetAllContentSettings())
            {
                generation.ContentSettings[contentSetting.NodeKey] = contentSetting;
            }

            var result = new List<SitemapNodeItem>();
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var startingNodes = new List<IPublishedContent>();
                if (context.RootNode != null)
                    startingNodes.Add(context.RootNode);
                else if (_documentNavigationQueryService.TryGetRootKeys(out var rootKeys))
                {
                    startingNodes.AddRange(rootKeys.Select(ctx.UmbracoContext.Content.GetById).Where(it => it != null));
                }

                foreach (var node in startingNodes)
                {
                    //We need to check the domains to figure out what cultures are valid for the alternate pages. Umbraco will otherwise just return the english variant for whatever reason
                    generation.ValidAlternateCultures.Clear();
                    if (generation.Settings.ShowAlternatePages)
                    {
                        generation.ValidAlternateCultures.AddRange(ctx.UmbracoContext.Domains.GetAssigned(node.Root().Id).Select(it => it.Culture));
                    }

                    var items = GetSelfAndChildren(generation, node, context.Culture);

                    _eventAggregator.Publish(new GenerateSitemapNotification(items));

                    result.AddRange(items);
                }
            }
            return result;
        }

        private List<SitemapNodeItem> GetSelfAndChildren(Generation generation, IPublishedContent content, string culture)
        {
            var items = new List<SitemapNodeItem>();
            var settings = generation.Settings;

            //Only show item if it actually has an template, so we don't index data objects and such
            if (content.TemplateId > 0 && !_publicAccessService.IsProtected(content.Path))
            {
                var docTypeSettings = GetPageTypeSettings(generation, content.ContentType.Key);
                generation.ContentSettings.TryGetValue(content.Key, out var contentOverride);

                // Resolve HideFromSitemap: excludeFromSitemap (content) → doc type → false.
                var hideFromSitemap = contentOverride?.ExcludeFromSitemap == true
                    || (docTypeSettings?.HideFromSitemap ?? false)
                    || (!string.IsNullOrWhiteSpace(culture) && !content.IsPublished(culture));

                var item = new SitemapNodeItem(BaseUrlHelper.ApplyBaseUrl(content.Url(culture, UrlMode.Absolute), generation.BaseUrl))
                {
                    HideFromSitemap = hideFromSitemap,
                    Content = content
                };

                if (!hideFromSitemap)
                {
                    if (!string.IsNullOrWhiteSpace(settings.LastModifiedFieldAlias) && HasValue(content, settings.LastModifiedFieldAlias, culture))
                        item.LastModifiedDate = Value<DateTime>(content, settings.LastModifiedFieldAlias, culture);
                    else
                        item.LastModifiedDate = content.UpdateDate;

                    // Resolve ChangeFrequency: content override → doc type → config field alias
                    var changeFrequency = contentOverride?.ChangeFrequency ?? docTypeSettings?.ChangeFrequency;
                    if (!string.IsNullOrWhiteSpace(changeFrequency))
                    {
                        item.ChangeFrequency = changeFrequency;
                    }
                    else if (!string.IsNullOrWhiteSpace(settings.ChangeFrequencyFieldAlias) && HasValue(content, settings.ChangeFrequencyFieldAlias, culture))
                    {
                        item.ChangeFrequency = Value<string>(content, settings.ChangeFrequencyFieldAlias, culture);
                    }

                    // Resolve Priority: content override → doc type → config field alias
                    var priority = contentOverride?.Priority ?? docTypeSettings?.Priority;
                    if (priority != null)
                    {
                        item.Priority = priority;
                    }
                    else if (!string.IsNullOrWhiteSpace(settings.PriorityFieldAlias) && HasValue(content, settings.PriorityFieldAlias, culture))
                    {
                        item.Priority = Value<double?>(content, settings.PriorityFieldAlias, culture);
                    }

                    if (settings.ShowAlternatePages)
                    {
                        var cultures = content.Cultures.Where(it => content.IsPublished(it.Key) && generation.ValidAlternateCultures.Contains(it.Key, StringComparer.InvariantCultureIgnoreCase)).ToArray();
                        if (cultures.Length > 1)
                        {
                            foreach (var additionalCulture in cultures)
                            {
                                item.AlternatePages.Add(new SitemapNodeAlternatePage(BaseUrlHelper.ApplyBaseUrl(content.Url(additionalCulture.Key, UrlMode.Absolute), generation.BaseUrl), additionalCulture.Key));
                            }
                        }
                    }

                    _eventAggregator.Publish(new GenerateSitemapNodeNotification(item));

                    items.Add(item);
                }
            }

            foreach (var child in content.Children(culture))
            {
                items.AddRange(GetSelfAndChildren(generation, child, culture));
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

        private SitemapPageSettings GetPageTypeSettings(Generation generation, Guid contentTypeId)
        {
            if (!generation.PageTypeSettings.ContainsKey(contentTypeId))
                generation.PageTypeSettings[contentTypeId] = _sitemapService.GetPageTypeSettings(contentTypeId);
            return generation.PageTypeSettings[contentTypeId];
        }

        //State of a single generation. The provider itself is a singleton, so nothing may be cached on the instance.
        private class Generation
        {
            public SitemapConfig Settings { get; }
            public string? BaseUrl { get; }
            public List<string> ValidAlternateCultures { get; } = new();
            public Dictionary<Guid, SitemapPageSettings> PageTypeSettings { get; } = new();
            public Dictionary<Guid, SitemapContentSettings> ContentSettings { get; } = new();

            public Generation(SitemapConfig settings, string? baseUrl)
            {
                Settings = settings;
                BaseUrl = baseUrl;
            }
        }
    }
}
