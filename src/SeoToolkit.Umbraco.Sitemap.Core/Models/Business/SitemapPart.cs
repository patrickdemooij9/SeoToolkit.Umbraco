using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Sitemap.Core.Models.Business
{
    /// <summary>
    /// A single sitemap file of a collection, for example "pages-2-sitemap.xml".
    /// </summary>
    public class SitemapPart
    {
        public string Alias { get; }
        public string DisplayName { get; }

        /// <summary>
        /// 1-based index of the part within its collection.
        /// </summary>
        public int Index { get; }

        public string Url { get; }
        public DateTime? LastModifiedDate { get; }
        public IReadOnlyList<SitemapNodeItem> Items { get; }

        public SitemapPart(string alias, string displayName, int index, string url, IReadOnlyList<SitemapNodeItem> items)
        {
            Alias = alias;
            DisplayName = displayName;
            Index = index;
            Url = url;
            Items = items;

            foreach (var item in items)
            {
                if (item.LastModifiedDate > LastModifiedDate || (LastModifiedDate is null && item.LastModifiedDate != null))
                    LastModifiedDate = item.LastModifiedDate;
            }
        }

        public static string GetFileName(string alias, int index) => $"{alias}-{index}-sitemap.xml";
    }
}
