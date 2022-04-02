using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels
{
    public class SiteAuditResultDetailViewModel
    {
        public string Message { get; set; }
        public int CheckId { get; set; }
        public bool IsError { get; set; }
        public bool IsWarning { get; set; }
    }
}
