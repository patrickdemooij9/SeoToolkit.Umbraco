namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels
{
    public class SiteAuditPageDetailViewModel
    {
        public string Url { get; set; }
        public int StatusCode { get; set; }
        public SiteAuditResultDetailViewModel[] Results { get; set; }
    }
}
