using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Deploy.Models
{
    /// <summary>The per-node SEO entities that currently have data for a content node.</summary>
    public class SeoTransferItemsViewModel
    {
        public IReadOnlyList<SeoTransferItem> Items { get; set; } = new List<SeoTransferItem>();
    }

    /// <summary>A single SEO deploy item: the content node key plus its SeoToolkit UDI entity type.</summary>
    public class SeoTransferItem(string id, string entityType)
    {
        public string Id { get; set; } = id;

        public string EntityType { get; set; } = entityType;
    }
}
