using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels
{
    public class SiteAuditCheckViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ErrorMessage { get; set; }
    }
}
