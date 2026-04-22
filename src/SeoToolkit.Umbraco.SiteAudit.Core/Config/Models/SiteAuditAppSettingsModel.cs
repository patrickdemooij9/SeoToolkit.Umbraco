using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Config.Models
{
    public class SiteAuditAppSettingsModel
    {
        public int MinimumDelayBetweenRequest { get; set; } = 1;
        public bool AllowMinimumDelayBetweenRequestSetting { get; set; } = false;
        public bool AllowInvalidCerts { get; set; } = false;
        public bool UseExternalCrawling { get; set; } = false;
        public string ExternalCrawlRequestEndpoint { get; set; }
        public string ExternalAuditDetailEndpoint { get; set; }
        public string ExternalStopAuditEndpoint { get; set; }
        public Dictionary<string, CheckAppSettingsModel> Checks { get; set; } = new Dictionary<string, CheckAppSettingsModel>();

        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}
