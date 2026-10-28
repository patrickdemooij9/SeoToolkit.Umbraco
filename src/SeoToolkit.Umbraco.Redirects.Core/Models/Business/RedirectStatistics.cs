namespace SeoToolkit.Umbraco.Redirects.Core.Models.Business
{
    public class RedirectStatistics
    {
        public int Total { get; set; }
        public int Enabled { get; set; }
        public int Permanent { get; set; }

        /// <summary>
        /// Enabled redirects hit on or after the "recent" date.
        /// </summary>
        public int RecentlyHit { get; set; }

        /// <summary>
        /// Enabled redirects whose last hit is before the "stale" date.
        /// </summary>
        public int Stale { get; set; }

        /// <summary>
        /// Enabled redirects without a recorded hit.
        /// </summary>
        public int NeverHit { get; set; }
    }
}
