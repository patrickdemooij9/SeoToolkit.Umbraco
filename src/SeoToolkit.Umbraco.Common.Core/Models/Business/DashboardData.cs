using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.Common.Core.Enums;

namespace SeoToolkit.Umbraco.Common.Core.Models.Business
{
    /// <summary>
    /// The data of a dashboard tile. Every data source returns this shape, so views can be reused across tiles.
    /// </summary>
    public class DashboardData
    {
        /// <summary>
        /// How good or bad the value is, decided by the data source.
        /// </summary>
        public DashboardDataStatus Status { get; set; }

        /// <summary>
        /// The main number of the tile.
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// What <see cref="Value"/> is out of, e.g. 1 of 2 redirects.
        /// </summary>
        public decimal? Total { get; set; }

        /// <summary>
        /// The main value when it isn't a number, e.g. "Yes".
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Context that only the data source knows, e.g. the date of the audit the value comes from.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// When the data was produced, e.g. the date of the audit. Formatted by the view in the user's locale.
        /// </summary>
        public DateTime? Date { get; set; }

        public List<DashboardDataItem> Items { get; set; } = new();
    }

    public class DashboardDataItem
    {
        public string Label { get; set; } = string.Empty;
        public decimal? Value { get; set; }
        public string? Text { get; set; }
        public DateTime? Date { get; set; }
        public DashboardDataStatus Status { get; set; }
        public string? Url { get; set; }
    }
}
