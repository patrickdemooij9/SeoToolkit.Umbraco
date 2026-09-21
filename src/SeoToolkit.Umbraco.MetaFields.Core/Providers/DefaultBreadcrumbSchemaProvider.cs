using Schema.NET;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Providers
{
    /// <summary>
    /// Generates a BreadcrumbList schema based on the position of the content in the content tree.
    /// </summary>
    public class DefaultBreadcrumbSchemaProvider : IBreadcrumbSchemaProvider
    {
        private readonly ISettingsService<MetaFieldsConfigModel> _settingsService;
        private readonly IDocumentNavigationQueryService _navigationQueryService;
        private readonly IPublishedContentStatusFilteringService _publishedStatusFilteringService;

        public DefaultBreadcrumbSchemaProvider(ISettingsService<MetaFieldsConfigModel> settingsService,
            IDocumentNavigationQueryService navigationQueryService,
            IPublishedContentStatusFilteringService publishedStatusFilteringService)
        {
            _settingsService = settingsService;
            _navigationQueryService = navigationQueryService;
            _publishedStatusFilteringService = publishedStatusFilteringService;
        }

        public IThing Get(IPublishedContent content)
        {
            if (content is null || content.ItemType != PublishedItemType.Content || !_settingsService.GetSettings().EnableBreadcrumbSchema)
                return null;

            // When the current page is rendered through a template, ancestors without a template
            // (such as folders) cannot be visited, so they are left out of the breadcrumb.
            var skipItemsWithoutTemplate = content.TemplateId > 0;

            var items = GetAncestorsOrSelf(content)
                .Reverse()
                .Where(item => !skipItemsWithoutTemplate || item.TemplateId > 0)
                .Select(item => (item.Name, Url: GetUrl(item)))
                .Where(item => !string.IsNullOrWhiteSpace(item.Name) && item.Url != null)
                .Select((item, index) => (IListItem)new ListItem
                {
                    Position = index + 1,
                    Name = item.Name,
                    Item = new WebPage { Id = item.Url }
                })
                .ToList();

            // A breadcrumb with only the current page does not add anything.
            if (items.Count < 2)
                return null;

            return new BreadcrumbList
            {
                ItemListElement = items
            };
        }

        /// <summary>
        /// Gets the content and its ancestors, starting with the content itself.
        /// </summary>
        protected virtual IEnumerable<IPublishedContent> GetAncestorsOrSelf(IPublishedContent content)
            => content.AncestorsOrSelf(_navigationQueryService, _publishedStatusFilteringService);

        protected virtual Uri GetUrl(IPublishedContent content)
        {
            var url = content.Url(mode: UrlMode.Absolute);
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
        }
    }
}
