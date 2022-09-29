using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels
{
    public class SiteAuditCreateConfigViewModel
    {
        public SiteAuditCheckViewModel[] Checks { get; set; }

        public int MinimumDelayBetweenRequest { get; set; }
        public bool AllowMinimumDelayBetweenRequestSetting { get; set; }

        public SiteAuditCreateConfigViewModel()
        {
            Checks = Array.Empty<SiteAuditCheckViewModel>();
        }
    }
}
