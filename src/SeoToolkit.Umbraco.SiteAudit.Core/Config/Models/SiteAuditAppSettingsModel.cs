using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Config.Models
{
    public class SiteAuditAppSettingsModel
    {
        public int MinimumDelayBetweenRequest { get; set; } = 1000;
        public bool AllowMinimumDelayBetweenRequestSetting { get; set; } = false;
        public Dictionary<string, CheckAppSettingsModel> Checks { get; set; } = new Dictionary<string, CheckAppSettingsModel>();
    }
}
