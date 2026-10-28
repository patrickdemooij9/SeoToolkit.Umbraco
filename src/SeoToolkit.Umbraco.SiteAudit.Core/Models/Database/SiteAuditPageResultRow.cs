namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Database
{
    /// <summary>
    /// A crawled page joined with one of its check results (no result when the page passed every check).
    /// </summary>
    public class SiteAuditPageResultRow
    {
        public int PageId { get; set; }
        public string Url { get; set; }
        public int? CheckId { get; set; }
        public int? ResultId { get; set; }
    }
}
