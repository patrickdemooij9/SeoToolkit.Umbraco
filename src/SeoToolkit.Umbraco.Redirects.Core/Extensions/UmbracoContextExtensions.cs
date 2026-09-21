using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.Redirects.Core.Extensions
{
    internal static class UmbracoContextExtensions
    {
        internal static IPublishedContent GetContentOrMediaByKey(this IUmbracoContext context,
            Guid key,
            IDocumentNavigationQueryService documentNavigationQueryService)
        {
            return documentNavigationQueryService.TryGetParentKey(key, out _)
                ? context.Content?.GetById(key)
                : context.Media?.GetById(key);
        }
    }
}
