using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.Sitemap.Core.Utils;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;

namespace SeoToolkit.Umbraco.Sitemap.Core.Interfaces
{
    /// <summary>
    /// Provides a set of items that is rendered as its own sitemap: "{Alias}-{index}-sitemap.xml".
    /// When a provider returns more items than allowed per sitemap, the items are split over multiple numbered sitemaps.
    /// </summary>
    public interface ISitemapCollectionProvider
    {
        /// <summary>
        /// URL-safe alias used in the sitemap file name. Lowercase letters, digits and hyphens only, and unique across all providers.
        /// Defaults to a kebab-cased version of the class name.
        /// </summary>
        string Alias => SitemapAliasHelper.FromType(GetType());

        /// <summary>
        /// Human readable name of the collection.
        /// </summary>
        string DisplayName => Alias;

        /// <summary>
        /// Whether this collection should be part of the sitemap for the given context (for example a specific domain).
        /// </summary>
        bool IsEnabled(SitemapCollectionContext context) => true;

        /// <summary>
        /// How long the items of this collection may be cached. Null or zero (the default) means the items are loaded on every request.
        /// Cached items are cleared whenever Umbraco content, domains, public access, document types or sitemap settings change,
        /// so only cache items that depend on Umbraco data, or pick a duration that matches how fresh your own data has to be.
        /// </summary>
        TimeSpan? CacheDuration => null;

        /// <summary>
        /// Returns all items for the given context. Splitting the items over multiple sitemaps is done by the generator.
        /// </summary>
        IEnumerable<SitemapNodeItem> GetItems(SitemapCollectionContext context)
        {
#pragma warning disable CS0618
            return GetItems();
#pragma warning restore CS0618
        }

        [Obsolete("Implement GetItems(SitemapCollectionContext) instead. This overload will be removed in a future major version.")]
        SitemapNodeItem[] GetItems() => Array.Empty<SitemapNodeItem>();
    }
}
